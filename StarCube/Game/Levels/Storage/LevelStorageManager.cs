using System;
using System.Collections.Generic;

using LiteDB;

using StarCube.Utility;
using StarCube.Utility.Container;
using StarCube.Data.Storage;
using StarCube.Game.Blocks;
using StarCube.Game.Levels.Chunks;
using StarCube.Game.Levels.Chunks.Storage;
using StarCube.Game.Levels.Chunks.Palette;

namespace StarCube.Game.Levels.Storage
{
    public sealed class LevelStorageManager : IDisposable
    {
        public const string LevelMetaDatabasePath = "level/meta";

        public const string LevelMetaCollectionName = "meta";

        public const string LevelDatabasePathPrefix = "level/";

        public const string BlockStatePaletteCollectionName = "palette_blockstate";

        /// <summary>
        /// 将一个 64 位整数转换为 level 的路径名
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string GuidToPath(Guid guid)
        {
            return LevelDatabasePathPrefix + guid.ToString("n");
        }


        /// <summary>
        /// 获取指定 guid 的 level 的 LevelStorage
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public LevelStorage OpenOrCreate(Guid guid)
        {
            CheckDisposed();
            lock (this)
            {
                // 检查是否已经被获取
                if (guidToLevelStorageCache.ContainsKey(guid))
                {
                    throw new ArgumentException("already acquired", nameof(guid));
                }

                string path = GuidToPath(guid);
                StorageDatabase database = saves.OpenOrCreateDatabase(path);
                return new LevelStorage(guid, this, database);
            }
        }

        /// <summary>
        /// 释放 LevelStorage
        /// </summary>
        /// <param name="storage"></param>
        internal void Release(LevelStorage storage)
        {
            CheckDisposed();
            lock (this)
            {
                if (!guidToLevelStorageCache.Remove(storage.guid))
                {
                    throw new ArgumentException($"tries to release LevelStorage (guid = {storage.guid}) which does not exist");
                }

                storage.database.Release();
            }
        }

        /// <summary>
        /// 删除指定 guid 的 level
        /// </summary>
        /// <param name="guid"></param>
        public bool Drop(Guid guid)
        {
            CheckDisposed();
            lock(this)
            {
                if (guidToLevelStorageCache.ContainsKey(guid))
                {
                    throw new ArgumentException($"tries to drop level (guid = {guid}) which is loaded");
                }

                string path = GuidToPath(guid);
                return saves.DropDatabase(path);
            }
        }


        public void Save()
        {
            blockStatePaletteManager.Save();
        }

        public void Dispose()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(LevelStorageManager), "double dispose");
            }

            foreach (LevelStorage storage in guidToLevelStorageCache.Values)
            {
                storage.Release();
            }
            guidToLevelStorageCache.Clear();

            levelMetaDatabase.Release();

            disposed = true;
        }

        private void CheckDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(LevelStorageManager), "disposed");
            }
        }

        public LevelStorageManager(GameSaves saves)
        {
            this.saves = saves;
            levelMetaDatabase = saves.OpenOrCreateDatabase(LevelMetaDatabasePath);
            levelMetaCollection = new Lazy<ILiteCollection<BsonDocument>>(() => levelMetaDatabase.Value.GetCollectionAndEnsureIndex(LevelMetaCollectionName, BsonAutoId.Guid));

            blockStatePaletteManager = new PaletteManager<BlockState>("blockstate", levelMetaDatabase, BlockState.GlobalBlockStateIDMap, BlockState.ToBson);
            chunkFactory = new PalettedChunkFactory(BlockState.GlobalBlockStateIDMap);
            chunkParser = new PalettedChunkParser(chunkFactory, blockStatePaletteManager);
        }

        private readonly GameSaves saves;

        private readonly StorageDatabase levelMetaDatabase;

        private readonly Lazy<ILiteCollection<BsonDocument>> levelMetaCollection;

        private readonly Dictionary<Guid, LevelStorage> guidToLevelStorageCache = new Dictionary<Guid, LevelStorage>();


        public readonly IChunkFactory chunkFactory;

        public readonly IChunkParser chunkParser;

        public readonly PaletteManager<BlockState> blockStatePaletteManager;


        private volatile bool disposed = false;
    }
}
