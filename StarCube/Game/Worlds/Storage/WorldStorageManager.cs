using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using LiteDB;

using StarCube.Utility;
using StarCube.Utility.Logging;
using StarCube.Data.Storage;

namespace StarCube.Game.Worlds.Storage
{
    public sealed class WorldStorageManager : IDisposable
    {
        public const string WorldMetaDatabasePath = "world/meta";

        public const string WorldMetaCollectionName = "meta";

        public const string WorldDatabasePathPrefix = "world/world_";

        public static string IndexToPath(long index)
        {
            return WorldDatabasePathPrefix + index.ToString("X").ToLower();
        }

        public bool Contains(Guid guid)
        {
            return worldMetaCollection.Exists(Query.EQ("_id", guid));
        }

        public WorldStorage GetOrCreate(Guid guid)
        {
            lock (this)
            {
                // 检查是否已经被获取
                if (idToWorldStorageCache.ContainsKey(guid))
                {
                    throw new ArgumentException("already acquired", nameof(guid));
                }

                // 尝试读取 level 的 meta 信息
                if (!TryGetWorldMeta(guid, out BsonDocument? meta))
                {
                    meta = new BsonDocument();
                    meta.Add("index", new BsonValue(GenerateNextWorldIndex()));
                    worldMetaCollection.Insert(guid, meta);
                }

                // 创建 LevelStorage
                long index = meta["index"].AsInt64;
                string path = IndexToPath(index);
                LiteDatabase db = saves.GetOrCreateDatabase(path);
                return new WorldStorage(path, db, guid, this);
            }
        }

        public void Release(WorldStorage storage)
        {
            lock (this)
            {
                if (!idToWorldStorageCache.Remove(storage.guid))
                {
                    LogUtil.Error($"tries to release WorldStorage (id = \"{storage.guid}\") which does not exist");
                    return;
                }

                saves.ReleaseDatabase(storage.path);
            }
        }

        public void Drop(Guid guid)
        {
            lock (this)
            {
                // 寻找并删除 level 的 meta 数据
                BsonDocument? meta = worldMetaCollection.FindById(guid);
                if (meta == null)
                {
                    return;
                }
                worldMetaCollection.Delete(guid);

                // 删除 level 的数据库
                long index = meta["index"].AsInt64;
                string levelPath = IndexToPath(index);
                saves.DropDatabase(levelPath);
            }
        }

        private bool TryGetWorldMeta(Guid guid, [NotNullWhen(true)] out BsonDocument? meta)
        {
            meta = worldMetaCollection.FindById(guid);
            return meta != null;
        }

        private long GenerateNextWorldIndex()
        {
            long current = nextWorldIndex;
            nextWorldIndex++;
            return current;
        }

        private long LoadNextWorldIndex()
        {
            BsonDocument? indexBson = worldMetaCollection.FindById(Guid.Empty);
            if (indexBson == null)
            {
                indexBson = new BsonDocument();
                indexBson.Add("current", new BsonValue(1L));
                worldMetaCollection.Insert(Guid.Empty, indexBson);
            }
            return indexBson["current"].AsInt64;
        }

        private void SaveNextWorldIndex()
        {
            BsonDocument indexBson = worldMetaCollection.FindById(Guid.Empty);
            indexBson["current"] = nextWorldIndex;
            worldMetaCollection.Update(indexBson);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            disposed = true;
        }

        ~WorldStorageManager()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
            {
                LogUtil.Error("WorldStorageManager disposed");
                throw new ObjectDisposedException(nameof(WorldStorageManager));
            }

            SaveNextWorldIndex();
            saves.ReleaseDatabase(WorldMetaDatabasePath);
            foreach (WorldStorage storage in idToWorldStorageCache.Values)
            {
                storage.Dispose();
            }

            if (disposing)
            {
                idToWorldStorageCache.Clear();
            }
        }


        public WorldStorageManager(GameSaves saves)
        {
            this.saves = saves;
            worldMetaDatabase = saves.GetOrCreateDatabase(WorldMetaDatabasePath);
            worldMetaCollection = worldMetaDatabase.GetCollectionAndEnsureIndex(WorldMetaCollectionName, BsonAutoId.Guid);
            nextWorldIndex = LoadNextWorldIndex();
        }

        private readonly GameSaves saves;

        private readonly LiteDatabase worldMetaDatabase;

        private readonly ILiteCollection<BsonDocument> worldMetaCollection;

        private long nextWorldIndex;

        private readonly Dictionary<Guid, WorldStorage> idToWorldStorageCache = new Dictionary<Guid, WorldStorage>();

        private volatile bool disposed = false;
    }
}
