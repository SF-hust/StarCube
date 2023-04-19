using System;

using LiteDB;
using StarCube.Data.Storage;

namespace StarCube.Game.Worlds.Storage
{
    public sealed class WorldStorage : GameStorage, IDisposable
    {
        public const string LevelCollectionName = "level";

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        internal WorldStorage(string path, LiteDatabase database, Guid guid, WorldStorageManager manager)
            : base(path, database)
        {
            this.guid = guid;
            this.manager = manager;
        }

        public readonly Guid guid;

        private readonly WorldStorageManager manager;
    }
}
