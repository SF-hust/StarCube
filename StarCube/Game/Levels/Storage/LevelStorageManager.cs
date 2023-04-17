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
                // 先从缓存中获取
                if (idToLevelStorageCache.TryGetValue(guid, out LevelStorage? levelStorage))
                {
                    return levelStorage;
                }

                // 再尝试读取 meta 中的信息
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
        /// 释放指定 guid 的 LevelStorage
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
            if (!collection.Exists(Query.EQ("_id", Guid.Empty)))
            {
                BsonDocument indexBson = new BsonDocument();
                indexBson.Add("current", new BsonValue(1u));
                collection.Insert(Guid.Empty, indexBson);
            }
            return collection;
        }

        private long GenerateNextIndex()
        {
            BsonDocument indexBson = levelMetaCollection.Value.FindById(Guid.Empty);
            long current = indexBson["current"].AsInt64;
            indexBson["current"] = current + 1;
            levelMetaCollection.Value.Upsert(indexBson);
            return current;
        }

        public void Dispose()
        {
            if (levelMetaDatabase.IsValueCreated)
            {
                saves.ReleaseDB(LevelMetaDBPath);
            }

            foreach (LevelStorage levelStorage in idToLevelStorageCache.Values)
            {
                levelStorage.Dispose();
            }
            idToLevelStorageCache.Clear();
        }

        public LevelStorageManager(GameSaves saves, IChunkParser chunkParser)
        {
            this.saves = saves;
            this.chunkParser = chunkParser;
            levelMetaDatabase = new Lazy<LiteDatabase>(GetLevelMetaDatabase, true);
            levelMetaCollection = new Lazy<ILiteCollection<BsonDocument>>(GetLevelMetaCollection, true);
        }

        private readonly GameSaves saves;

        private readonly IChunkParser chunkParser;

        private readonly Lazy<LiteDatabase> levelMetaDatabase;

        private readonly Lazy<ILiteCollection<BsonDocument>> levelMetaCollection;

        private readonly Dictionary<Guid, LevelStorage> idToLevelStorageCache = new Dictionary<Guid, LevelStorage>();
    }
}
