using System;
using System.Collections.Generic;
using System.Linq;

using LiteDB;

using StarCube.Utility;
using StarCube.Utility.Logging;
using StarCube.Data.Storage;
using StarCube.Data.Storage.Exceptions;

namespace StarCube.Game.Worlds.Storage
{
    public sealed class WorldStorageManager : IDisposable
    {
        public const string WorldMetaDatabasePath = "world/meta";

        public const string WorldMetaCollectionName = "meta";

        public const string WorldActiveField = "active";

        public const string WorldDatabasePathPrefix = "world/";

        /// <summary>
        /// 根据 world 的 guid 生成对应 world 存储的数据库的路径
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static string GuidToPath(Guid guid)
        {
            return WorldDatabasePathPrefix + guid.ToString("n");
        }

        /// <summary>
        /// 读取活跃的 world 列表
        /// </summary>
        /// <returns></returns>
        public List<Guid> LoadActiveWorldList()
        {
            CheckDisposed();

            if (!worldMetaDatabase.Created)
            {
                return new List<Guid>();
            }

            return worldMetaCollection.Value
                .Find(Query.EQ(WorldActiveField, new BsonValue(true)))
                .Select((bson) => bson["_id"].AsGuid)
                .ToList();
        }

        /// <summary>
        /// 查询指定 guid 的 ServerWorldStorage 是否存在
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public bool Contains(Guid guid)
        {
            CheckDisposed();
            lock (this)
            {
                // 检查指定的 world 是否已打开或者其元数据已修改
                if (guidToOpenedWorldStorageCache.ContainsKey(guid) ||
                    guidToModifiedWorldMetaCache.ContainsKey(guid))
                {
                    return true;
                }
            }

            // 检查数据库中是否有相应 world 的元数据
            if (worldMetaDatabase.Created && worldMetaCollection.Value.Exists(Query.EQ("_id", guid)))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 打开或创建 guid 对应的 ServerWorldStorage
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public WorldStorage OpenOrCreate(Guid guid)
        {
            CheckDisposed();
            lock (this)
            {
                // 检查 ServerWorldStorage 是否已经被打开
                if (guidToOpenedWorldStorageCache.ContainsKey(guid))
                {
                    throw new ArgumentException("already acquired", nameof(guid));
                }

                // world 的元数据已修改，更新其元数据
                if (guidToModifiedWorldMetaCache.TryGetValue(guid, out var meta))
                {
                    meta[WorldActiveField] = true;
                }
                // world 的元数据存在数据库中，更新其元数据
                else if (worldMetaDatabase.Created && worldMetaCollection.Value.FindById(guid) != null)
                {
                    meta = worldMetaCollection.Value.FindById(guid);
                    meta[WorldActiveField] = true;
                    guidToModifiedWorldMetaCache.Add(guid, meta);
                }
                // world 的元数据不存在，创建一份新的
                else
                {
                    meta = new BsonDocument();
                    meta["_id"] = guid;
                    meta[WorldActiveField] = true;
                    guidToModifiedWorldMetaCache.Add(guid, meta);
                }

                // 创建 ServerWorldStorage
                string path = GuidToPath(guid);
                StorageDatabase database = saves.OpenOrCreateDatabase(path);
                var worldStorage = new WorldStorage(guid, this, database);
                guidToOpenedWorldStorageCache.Add(guid, worldStorage);
                return worldStorage;
            }
        }

        /// <summary>
        /// 关闭一个 ServerWorldStorage
        /// </summary>
        /// <param name="guid"></param>
        internal void Release(Guid guid)
        {
            CheckDisposed();
            lock (this)
            {
                // 检查是否存在对应 guid 的数据库
                if (!guidToOpenedWorldStorageCache.Remove(guid, out var worldStorage))
                {
                    LogUtil.Fatal($"tries to release WorldStorage (id = \"{guid}\") which does not exist");
                    throw new KeyNotFoundException(nameof(guid));
                }

                // 修改其元数据
                if (guidToModifiedWorldMetaCache.TryGetValue(guid, out var meta))
                {
                    meta[WorldActiveField] = false;
                }
                else if (worldMetaDatabase.Created && worldMetaCollection.Value.FindById(guid) != null)
                {
                    meta = worldMetaCollection.Value.FindById(guid);
                    meta[WorldActiveField] = false;
                    guidToModifiedWorldMetaCache.Add(guid, meta);
                }
                // 没找到对应的元数据
                else
                {
                    throw new GameSavesCorruptException(nameof(WorldStorageManager));
                }

                // 关闭对应的数据库
                worldStorage.database.Release();
            }
        }

        /// <summary>
        /// 删除指定 guid 的 ServerWorldStorage 的存档记录与文件，此时其必须没有被打开
        /// </summary>
        /// <param name="guid"></param>
        public bool Drop(Guid guid)
        {
            CheckDisposed();
            lock (this)
            {
                // 检查 world 的 storage 是否已被打开，已打开的 storage 不能被 drop
                if (guidToOpenedWorldStorageCache.ContainsKey(guid))
                {
                    return false;
                }

                // 删除尚未保存的元数据缓存
                bool found = guidToModifiedWorldMetaCache.Remove(guid);

                // 删除数据库中已保存的元数据
                if (worldMetaDatabase.Created)
                {
                    found |= worldMetaCollection.Value.Delete(guid);
                }

                // 删除 world 的数据库
                if (found)
                {
                    string path = GuidToPath(guid);
                    saves.DropDatabase(path);
                }

                return found;
            }
        }

        /// <summary>
        /// 将元数据保存到数据库中
        /// </summary>
        public void Save()
        {
            CheckDisposed();

            // 保存并清除已修改的 world 元数据
            worldMetaCollection.Value.Upsert(guidToModifiedWorldMetaCache.Values);
            guidToModifiedWorldMetaCache.Clear();
        }


        public void Dispose()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(WorldStorageManager), "double dispose");
            }

            // 关闭 world 元数据的数据库
            worldMetaDatabase.Release();

            // 关闭所有打开的 ServerWorldStorage
            foreach (WorldStorage storage in guidToOpenedWorldStorageCache.Values.ToList())
            {
                storage.Release();
            }
            guidToOpenedWorldStorageCache.Clear();

            disposed = true;
        }

        private void CheckDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(WorldStorageManager), "disposed");
            }
        }

        public WorldStorageManager(GameSaves saves)
        {
            this.saves = saves;
            worldMetaDatabase = saves.OpenOrCreateDatabase(WorldMetaDatabasePath);
            worldMetaCollection = new Lazy<ILiteCollection<BsonDocument>>(() =>
            {
                var collection = worldMetaDatabase.Value.GetCollectionAndEnsureIndex(WorldMetaCollectionName, BsonAutoId.Guid);
                collection.EnsureIndex(WorldActiveField);
                return collection;
            });
        }

        private readonly GameSaves saves;

        private readonly StorageDatabase worldMetaDatabase;

        private readonly Lazy<ILiteCollection<BsonDocument>> worldMetaCollection;

        /// <summary>
        /// 所有已打开的 ServerWorldStorage
        /// </summary>
        private readonly Dictionary<Guid, WorldStorage> guidToOpenedWorldStorageCache = new Dictionary<Guid, WorldStorage>();

        /// <summary>
        /// 自上次保存以来修改过的 world 元数据
        /// </summary>
        private readonly Dictionary<Guid, BsonDocument> guidToModifiedWorldMetaCache = new Dictionary<Guid, BsonDocument>();

        private volatile bool disposed = false;
    }
}
