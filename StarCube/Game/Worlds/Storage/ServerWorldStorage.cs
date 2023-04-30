using System;

using LiteDB;

using StarCube.Utility;
using StarCube.Utility.Logging;
using StarCube.Data.Storage;
using StarCube.Data.Storage.Exceptions;
using StarCube.Server.Game;

namespace StarCube.Game.Worlds.Storage
{
    public sealed class ServerWorldStorage
    {
        public const string WorldMetaCollectionName = "meta";

        public const string TotalTickCountField = "tick";

        public const string WorldCollectionName = "world";


        /// <summary>
        /// 加载 world 的总 tick 时长
        /// </summary>
        /// <returns></returns>
        /// <exception cref="GameSavesCorruptException"></exception>
        public long LoadTotalTickCount()
        {
            CheckReleased();

            if (!database.Created)
            {
                return 0L;
            }

            var meta = worldMetaCollection.Value.FindById(0) ?? throw new GameSavesCorruptException("world total tick");
            return meta[TotalTickCountField].AsInt64;
        }

        /// <summary>
        /// 保存 world 的总 tick 时长
        /// </summary>
        /// <param name="totalTickCount"></param>
        public void SaveTotalTickCount(long totalTickCount)
        {
            CheckReleased();

            var meta = worldMetaCollection.Value.FindById(0) ?? new BsonDocument();
            meta[TotalTickCountField] = totalTickCount;
            worldMetaCollection.Value.Upsert(0, meta);
        }


        public void Release()
        {
            if (released)
            {
                LogUtil.Error("ServerGameStorage disposed");
                throw new ObjectDisposedException(nameof(GameStorage));
            }

            manager.Release(guid);

            released = true;
        }

        private void CheckReleased()
        {
            if (released)
            {
                throw new ObjectDisposedException(nameof(GameStorage), "disposed");
            }
        }


        internal ServerWorldStorage(Guid guid, ServerWorldStorageManager manager, StorageDatabase database)
        {
            this.guid = guid;
            this.manager = manager;
            this.database = database;
            worldMetaCollection = new Lazy<ILiteCollection<BsonDocument>>(() => database.Value.GetCollectionAndEnsureIndex(WorldMetaCollectionName, BsonAutoId.Int32));
        }

        public readonly Guid guid;

        private readonly ServerWorldStorageManager manager;

        public readonly StorageDatabase database;

        private readonly Lazy<ILiteCollection<BsonDocument>> worldMetaCollection;

        private volatile bool released = false;
    }
}
