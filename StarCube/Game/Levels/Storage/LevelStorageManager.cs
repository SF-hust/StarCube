using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using LiteDB;

using StarCube.Utility.Logging;
using StarCube.Data.Storage;
using StarCube.Game.Levels.Chunks.Storage;

namespace StarCube.Game.Levels.Storage
{
    public sealed class LevelStorageManager : IDisposable
    {
        public const string LevelMetaDBPath = "level/meta";

        public const string LevelDBPathPrefix = "level/level_";

        /// <summary>
        /// 将一个 64 位整数转换为 level 的路径名
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string IndexToPath(long index)
        {
            return LevelDBPathPrefix + index.ToString("X").ToLower();
        }


        /// <summary>
        /// 获取指定 guid 的 level 的 LevelStorage
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public LevelStorage GetOrCreate(Guid guid)
        {
            lock(this)
            {
                // 检查是否已经被获取
                if (idToLevelStorageCache.ContainsKey(guid))
                {
                    throw new ArgumentException("already acquired", nameof(guid));
                }

                // 尝试读取 level 的 meta 信息
                if (!TryGetLevelMeta(guid, out BsonDocument? meta))
                {
                    meta = new BsonDocument();
                    meta.Add("index", new BsonValue(GenerateNextIndex()));
                    levelMetaCollection.Value.Insert(guid, meta);
                }

                // 创建 LevelStorage
                long index = meta["index"].AsInt64;
                string path = IndexToPath(index);
                LiteDatabase db = saves.GetOrCreateDB(path);
                return new LevelStorage(guid, path, db, this, chunkParser);
            }
        }

        /// <summary>
        /// 释放 LevelStorage
        /// </summary>
        /// <param name="levelStorage"></param>
        public void Release(LevelStorage levelStorage)
        {
            lock(this)
            {
                if (!idToLevelStorageCache.Remove(levelStorage.guid))
                {
                    LogUtil.Error($"tries to release levelStorage (id = \"{levelStorage.guid}\") which does not exist");
                    return;
                }

                saves.ReleaseDB(levelStorage.path);
            }
        }

        /// <summary>
        /// 删除指定 guid 的 level
        /// </summary>
        /// <param name="guid"></param>
        public void Drop(Guid guid)
        {
            lock(this)
            {
                // 寻找并删除 level 的 meta 数据
                BsonDocument? meta = levelMetaCollection.Value.FindById(guid);
                if (meta == null)
                {
                    return;
                }
                levelMetaCollection.Value.Delete(guid);

                // 删除 level 的数据库
                long index = meta["index"].AsInt64;
                string levelPath = IndexToPath(index);
                saves.DropDB(levelPath);
            }
        }


        private bool TryGetLevelMeta(Guid guid, [NotNullWhen(true)] out BsonDocument? meta)
        {
            meta = levelMetaCollection.Value.FindById(guid);
            return meta != null;
        }

        private LiteDatabase GetLevelMetaDatabase()
        {
            return saves.GetOrCreateDB(LevelMetaDBPath);
        }

        private ILiteCollection<BsonDocument> GetLevelMetaCollection()
        {
            ILiteCollection<BsonDocument> collection = levelMetaDatabase.Value.GetCollection("level", BsonAutoId.Guid);
            collection.EnsureIndex("_id");
            return collection;
        }

        private long GenerateNextIndex()
        {
            long current = currentIndex;
            currentIndex++;
            return current;
        }

        private long GetCurrentIndex()
        {
            BsonDocument? indexBson = levelMetaCollection.Value.FindById(Guid.Empty);
            if (indexBson == null)
            {
                indexBson = new BsonDocument();
                indexBson.Add("current", new BsonValue(1L));
                return 1L;
            }
            return indexBson["current"].AsInt64;
        }

        private void SaveCurrentIndex()
        {
            BsonDocument indexBson = levelMetaCollection.Value.FindById(Guid.Empty);
            indexBson["current"] = currentIndex;
            levelMetaCollection.Value.Update(indexBson);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            disposed = true;
        }

        ~LevelStorageManager()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (levelMetaDatabase.IsValueCreated)
            {
                saves.ReleaseDB(LevelMetaDBPath);
            }
            foreach (LevelStorage levelStorage in idToLevelStorageCache.Values)
            {
                levelStorage.Dispose();
            }
            SaveCurrentIndex();

            if (disposing)
            {
                idToLevelStorageCache.Clear();
            }
        }


        public LevelStorageManager(GameSaves saves, IChunkParser chunkParser)
        {
            this.saves = saves;
            this.chunkParser = chunkParser;
            levelMetaDatabase = new Lazy<LiteDatabase>(GetLevelMetaDatabase, true);
            levelMetaCollection = new Lazy<ILiteCollection<BsonDocument>>(GetLevelMetaCollection, true);
            currentIndex = GetCurrentIndex();
        }

        private readonly GameSaves saves;

        private readonly IChunkParser chunkParser;

        private long currentIndex;

        private readonly Lazy<LiteDatabase> levelMetaDatabase;

        private readonly Lazy<ILiteCollection<BsonDocument>> levelMetaCollection;

        private readonly Dictionary<Guid, LevelStorage> idToLevelStorageCache = new Dictionary<Guid, LevelStorage>();

        private bool disposed = false;
    }
}
