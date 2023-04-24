using System;

using LiteDB;

using StarCube.Utility;
using StarCube.Utility.Logging;
using StarCube.Data.Storage;
using StarCube.Data.Storage.Exceptions;

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

            // database 尚未创建，说明是个新创建的 world，返回 0L
            if (!database.Created)
            {
                return 0L;
            }

            // 从存档中读取
            var meta = worldMetaCollection.Value.FindById(0);
            if (meta != null && meta.TryGetInt64(TotalTickCountField, out long totalTickCount))
            {
                return totalTickCount;
            }
            throw new GameSavesCorruptException("world total tick");
        }

        /// <summary>
        /// 保存 world 的总 tick 时长
        /// </summary>
        /// <param name="totalTickCount"></param>
        public void SaveTotalTickCount(long totalTickCount)
        {
            CheckReleased();

            var meta = worldMetaCollection.Value.FindById(0);
            if (meta == null)
            {
                meta = new BsonDocument();
                meta["_id"] = new BsonValue(0);
            }
            meta[TotalTickCountField] = new BsonValue(totalTickCount);
            worldMetaCollection.Value.Update(meta);
        }


        public void Release()
        {
            if (released)
            {
                LogUtil.Error("ServerGameStorage disposed");
                throw new ObjectDisposedException(nameof(ServerGameStorage));
            }

            manager.Release(guid);

            released = true;
        }

        private void CheckReleased()
        {
            if (released)
            {
                throw new ObjectDisposedException(nameof(ServerGameStorage), "disposed");
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
