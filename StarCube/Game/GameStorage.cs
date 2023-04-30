using System;
using System.Numerics;

using LiteDB;

using StarCube.Utility;
using StarCube.Data.Storage;
using StarCube.Data.Storage.Exceptions;

namespace StarCube.Game
{
    public sealed class GameStorage : IDisposable
    {
        public const string GameMetaDatabasePath = "meta";

        public const string GameMetaCollectionName = "meta";

        public const string PlayerCollectionName = "player";

        public const string NameField = "name";

        public const string TotalTickCountField = "tick";

        public const string PositionField = "pos";

        public const string WorldField = "world";

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

        /// <summary>
        /// 根据 Guid 读取玩家信息
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="name"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool TryLoadPlayer(Guid guid, out string name, out Guid worldGuid, out Vector3 position)
        {
            name = string.Empty;
            worldGuid = Guid.Empty;
            position = Vector3.Zero;
            if (!playerCollection.IsValueCreated)
            {
                return false;
            }

            BsonDocument? bson = playerCollection.Value.FindById(guid);
            if (bson == null)
            {
                return false;
            }
            name = bson[NameField];
            worldGuid = bson[WorldField];
            bson.TryGetVector3(PositionField, out position);
            return true;
        }

        /// <summary>
        /// 根据 name 读取玩家信息
        /// </summary>
        /// <param name="name"></param>
        /// <param name="guid"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool TryLoadPlayer(string name, out Guid guid, out Guid worldGuid, out Vector3 position)
        {
            guid = Guid.Empty;
            worldGuid = Guid.Empty;
            position = Vector3.Zero;
            if (!playerCollection.IsValueCreated)
            {
                return false;
            }

            BsonDocument? bson = playerCollection.Value.FindOne(Query.EQ(NameField, name));
            if (bson == null)
            {
                return false;
            }
            guid = bson["_id"];
            worldGuid = bson[WorldField];
            bson.TryGetVector3(PositionField, out position);
            return true;
        }

        /// <summary>
        /// 保存玩家信息
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="name"></param>
        /// <param name="position"></param>
        public void SavePlayer(Guid guid, string name, Guid worldGuid, Vector3 position)
        {
            playerCollection.Value.EnsureIndex(NameField);
            BsonDocument bson = new BsonDocument();
            bson.Add(NameField, name);
            bson.Add(WorldField, worldGuid);
            bson.Add(PositionField, position);
            playerCollection.Value.Upsert(guid, bson);
        }

        /// <summary>
        /// 删除玩家信息
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public bool DropPlayer(Guid guid)
        {
            return playerCollection.Value.Delete(guid);
        }

        public void Dispose()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(GameStorage));
            }

            gameMetaDatabase.Release();

            disposed = true;
        }

        private void CheckDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(GameStorage), "disposed");
            }
        }

        public GameStorage(GameSaves saves)
        {
            this.saves = saves;
            gameMetaDatabase = saves.OpenOrCreateDatabase(GameMetaDatabasePath);
            gameMetaCollection = new Lazy<ILiteCollection<BsonDocument>>(() => gameMetaDatabase.Value.GetCollectionAndEnsureIndex(GameMetaCollectionName, BsonAutoId.Int32));

            playerCollection = new Lazy<ILiteCollection<BsonDocument>>(() => gameMetaDatabase.Value.GetCollectionAndEnsureIndex(PlayerCollectionName, BsonAutoId.Guid));
        }

        private readonly GameSaves saves;

        private readonly StorageDatabase gameMetaDatabase;

        private readonly Lazy<ILiteCollection<BsonDocument>> gameMetaCollection;

        private readonly Lazy<ILiteCollection<BsonDocument>> playerCollection;

        private volatile bool disposed = false;
    }
}
