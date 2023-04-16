using System.Diagnostics.CodeAnalysis;

using System;

using LiteDB;

using StarCube.Utility.Math;
using StarCube.Game.Levels.Chunks;
using StarCube.Game.Levels.Chunks.Storage;

namespace StarCube.Game.Levels.Storage
{
    public sealed class LevelStorage : ILevelStorage
    {
        public bool Contains(ChunkPos pos)
        {
            return chunkCollection.Value.Exists(Query.EQ("_id", pos.ToLiteDBObjectID()));
        }

        public bool TryLoadChunk(ChunkPos pos, [NotNullWhen(true)] out Chunk? chunk)
        {
            BsonDocument? bson = chunkCollection.Value.FindById(pos.ToLiteDBObjectID());
            if (bson == null)
            {
                chunk = null;
                return false;
            }
            return chunkParser.TryParse(bson, pos, out chunk);
        }

        public void WriteChunk(Chunk chunk)
        {
            BsonDocument bson = chunkParser.ToBson(chunk);
            chunkCollection.Value.Upsert(chunk.pos.ToLiteDBObjectID(), bson);
        }

        private ILiteCollection<BsonDocument> GetChunkCollection()
        {
            ILiteCollection<BsonDocument> chunkCollection = database.GetCollection("chunks", BsonAutoId.ObjectId);
            chunkCollection.EnsureIndex("_id");

            return chunkCollection;
        }

        public void Dispose()
        {
            levelStorageManager.Release(this);
        }

        internal LevelStorage(Guid guid, string path, LiteDatabase database, LevelStorageManager levelStorageManager, IChunkParser chunkParser)
        {
            this.guid = guid;
            this.path = path;
            this.database = database;
            this.levelStorageManager = levelStorageManager;
            this.chunkParser = chunkParser;
            chunkCollection = new Lazy<ILiteCollection<BsonDocument>>(GetChunkCollection, true);
        }

        public readonly Guid guid;

        public readonly string path;

        private readonly LiteDatabase database;

        private readonly LevelStorageManager levelStorageManager;

        private readonly IChunkParser chunkParser;

        private readonly Lazy<ILiteCollection<BsonDocument>> chunkCollection;
    }
}
