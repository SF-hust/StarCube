using System;

using LiteDB;

using StarCube.Data.Storage;
using StarCube.Data.Storage.Exceptions;
using StarCube.Utility;

namespace StarCube.Game
{
    public sealed class ServerGameStorage : IDisposable
    {
        public const string GameMetaDatabasePath = "meta";

        public const string GameMetaCollectionName = "meta";

        public const string NameField = "name";

        public const string TotalTickCountField = "tick";

        /// <summary>
        /// 读取 ServerGame 的总 tick 时间
        /// </summary>
        /// <returns></returns>
        /// <exception cref="GameSavesCorruptException"></exception>
        public long LoadTotalTickCount()
        {
            CheckDisposed();

            if (!gameMetaDatabase.Created)
            {
                return 0L;
            }

            var meta = gameMetaCollection.Value.FindById(0) ?? throw new GameSavesCorruptException("missing server game meta data");
            return meta[TotalTickCountField].AsInt64;
        }

        /// <summary>
        /// 保存 ServerGame 的总 tick 时间
        /// </summary>
        /// <param name="totalTickCount"></param>
        public void SaveTotalTickCount(long totalTickCount)
        {
            CheckDisposed();

            if (!gameMetaDatabase.Created)
            {
                BsonDocument bson = new BsonDocument();
                bson[NameField] = saves.name;
                bson[TotalTickCountField] = totalTickCount;
                gameMetaCollection.Value.Insert(0, bson);
                return;
            }

            var meta = gameMetaCollection.Value.FindById(0) ?? throw new GameSavesCorruptException("missing server game meta data");
            meta[TotalTickCountField] = totalTickCount;
            gameMetaCollection.Value.Upsert(meta);
        }

        public void Dispose()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(ServerGameStorage));
            }

            saves.ReleaseDatabase(gameMetaDatabase);

            disposed = true;
        }

        private void CheckDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(ServerGameStorage), "disposed");
            }
        }

        public ServerGameStorage(GameSaves saves)
        {
            this.saves = saves;
            gameMetaDatabase = saves.OpenOrCreateDatabase(GameMetaDatabasePath);
            gameMetaCollection = new Lazy<ILiteCollection<BsonDocument>>(() => gameMetaDatabase.Value.GetCollectionAndEnsureIndex(GameMetaCollectionName, BsonAutoId.Int32));
        }

        private readonly GameSaves saves;

        private readonly StorageDatabase gameMetaDatabase;

        private readonly Lazy<ILiteCollection<BsonDocument>> gameMetaCollection;

        private volatile bool disposed = false;
    }
}
