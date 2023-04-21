using System;
using System.Diagnostics;

using LiteDB;
using StarCube.Data.Storage;
using StarCube.Utility.Logging;

namespace StarCube.Game
{
    public sealed class ServerGameStorage : IDisposable
    {
        public const string GameMetaDatabasePath = "meta";

        public const string GameMetaCollectionName = "meta";

        public const string TotalTickCount = "tick";

        public long LoadTotalTickCount()
        {
            Debug.Assert(!disposed);

            var collection = gameMetaDatabase.GetCollection(GameMetaCollectionName);
            var meta = collection.FindOne(Query.All()) ?? throw new Exception();
            return meta[TotalTickCount].AsInt64;
        }

        public void SaveTotalTickCount(long totalTickCount)
        {
            Debug.Assert(!disposed);

            var collection = gameMetaDatabase.GetCollection(GameMetaCollectionName);
            var meta = collection.FindOne(Query.All()) ?? throw new Exception();
            meta[TotalTickCount] = totalTickCount;
            collection.Update(meta);
        }

        public void Dispose()
        {
            if (disposed)
            {
                LogUtil.Error("ServerGameStorage disposed");
                throw new ObjectDisposedException(nameof(ServerGameStorage));
            }

            saves.ReleaseDatabase(GameMetaDatabasePath);

            disposed = true;
        }

        public ServerGameStorage(GameSaves saves)
        {
            this.saves = saves;
            gameMetaDatabase = saves.GetOrCreateDatabase(GameMetaDatabasePath);
        }

        private readonly GameSaves saves;

        private readonly LiteDatabase gameMetaDatabase;

        private volatile bool disposed = false;
    }
}
