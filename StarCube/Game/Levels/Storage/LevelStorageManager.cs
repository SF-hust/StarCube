using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using LiteDB;

using StarCube.Utility;
using StarCube.Utility.Logging;
using StarCube.Data.Storage;
using StarCube.Game.Blocks;
using StarCube.Game.Levels.Chunks.Storage;
using StarCube.Game.Levels.Chunks.Storage.Palette;
using StarCube.Game.Levels.Chunks;

namespace StarCube.Game.Levels.Storage
{
    public sealed class LevelStorageManager : IDisposable
    {
        public const string LevelMetaDatabasePath = "level/meta";

        public const string LevelMetaCollectionName = "meta";

        public const string LevelDatabasePathPrefix = "level/level_";

        public const string BlockStatePaletteCollectionName = "palette_blockstate";

        /// <summary>
        /// 将一个 64 位整数转换为 level 的路径名
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string IndexToPath(long index)
        {
            return LevelDatabasePathPrefix + index.ToString("X").ToLower();
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
                    meta.Add("index", new BsonValue(GenerateNextLevelIndex()));
                    levelMetaCollection.Insert(guid, meta);
                }

                // 创建 LevelStorage
                long index = meta["index"].AsInt64;
                string path = IndexToPath(index);
                StorageDatabase database = saves.OpenOrCreateDatabase(path);
                return new LevelStorage(guid, this, database);
            }
        }

        /// <summary>
        /// 释放 LevelStorage
        /// </summary>
        /// <param name="storage"></param>
        public void Release(LevelStorage storage)
        {
            lock(this)
            {
                if (!idToLevelStorageCache.Remove(storage.guid))
                {
                    LogUtil.Error($"tries to release LevelStorage (id = \"{storage.guid}\") which does not exist");
                    return;
                }

                storage.database.Release();
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
                BsonDocument? meta = levelMetaCollection.FindById(guid);
                if (meta == null)
                {
                    return;
                }
                levelMetaCollection.Delete(guid);

                // 删除 level 的数据库
                long index = meta["index"].AsInt64;
                string levelPath = IndexToPath(index);
                saves.DropDatabase(levelPath);
            }
        }


        private bool TryGetLevelMeta(Guid guid, [NotNullWhen(true)] out BsonDocument? meta)
        {
            meta = levelMetaCollection.FindById(guid);
            return meta != null;
        }

        private long GenerateNextLevelIndex()
        {
            long current = nextLevelIndex;
            nextLevelIndex++;
            return current;
        }

        private long LoadNextLevelIndex()
        {
            BsonDocument? indexBson = levelMetaCollection.FindById(Guid.Empty);
            if (indexBson == null)
            {
                indexBson = new BsonDocument();
                indexBson.Add("current", new BsonValue(1L));
                levelMetaCollection.Insert(Guid.Empty, indexBson);
            }
            return indexBson["current"].AsInt64;
        }

        private void SaveNextLevelIndex()
        {
            BsonDocument indexBson = levelMetaCollection.FindById(Guid.Empty);
            indexBson["current"] = nextLevelIndex;
            levelMetaCollection.Update(indexBson);
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
                LogUtil.Error("LevelStorageManager disposed");
                throw new ObjectDisposedException(nameof(LevelStorageManager));
            }

            SaveNextLevelIndex();
            levelMetaDatabase.Release();
            foreach (LevelStorage storage in idToLevelStorageCache.Values)
            {
                storage.Dispose();
            }

            if (disposing)
            {
                idToLevelStorageCache.Clear();
            }
        }


        public LevelStorageManager(GameSaves saves)
        {
            this.saves = saves;
            levelMetaDatabase = saves.OpenOrCreateDatabase(LevelMetaDatabasePath);
            levelMetaCollection = levelMetaDatabase.Value.GetCollectionAndEnsureIndex(LevelMetaCollectionName, BsonAutoId.Guid);
            nextLevelIndex = LoadNextLevelIndex();
            ILiteCollection<BsonDocument> blockStatePaletteCollection = levelMetaDatabase.Value.GetCollectionAndEnsureIndex(BlockStatePaletteCollectionName, BsonAutoId.Int32);
            PaletteManager<BlockState> blockStatePaletteManager = PaletteManager<BlockState>.Load("blockstate", BlockState.GlobalBlockStateIDMap, blockStatePaletteCollection, BlockState.ToBson, saves.name);
            chunkFactory = new PalettedChunkFactory(BlockState.GlobalBlockStateIDMap);
            chunkParser = new PalettedChunkParser(chunkFactory, blockStatePaletteManager);
        }

        private readonly GameSaves saves;

        private readonly StorageDatabase levelMetaDatabase;

        private readonly ILiteCollection<BsonDocument> levelMetaCollection;

        private long nextLevelIndex;

        public readonly IChunkParser chunkParser;

        public readonly IChunkFactory chunkFactory;

        private readonly Dictionary<Guid, LevelStorage> idToLevelStorageCache = new Dictionary<Guid, LevelStorage>();

        private volatile bool disposed = false;
    }
}
