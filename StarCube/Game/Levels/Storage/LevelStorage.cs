using System.Diagnostics.CodeAnalysis;

using System;

using LiteDB;

using StarCube.Utility.Math;
using StarCube.Game.Levels.Chunks;
using StarCube.Game.Levels.Chunks.Storage;
using StarCube.Game.Levels.Chunks.Loading;
using System.Collections.Generic;

namespace StarCube.Game.Levels.Storage
{
    public sealed class LevelStorage
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

        public Dictionary<long, AnchorData> LoadStaticLevelAnchors()
        {
            Dictionary<long, AnchorData> idToAnchorData = new Dictionary<long, AnchorData>();
            return idToAnchorData;
        }

        public void WriteStaticLevelAnchors(Dictionary<long, AnchorData> idToAnchorData)
        {

        }

        private ILiteCollection<BsonDocument> GetChunkCollection()
        {
            ILiteCollection<BsonDocument> chunkCollection = database.GetCollection("chunks", BsonAutoId.ObjectId);
            chunkCollection.EnsureIndex("_id");

            return chunkCollection;
        }

        private ILiteCollection<BsonDocument> GetStaticAnchorDataCollection()
        {
            ILiteCollection<BsonDocument> chunkCollection = database.GetCollection("anchors", BsonAutoId.Int64);
            chunkCollection.EnsureIndex("_id");

            return chunkCollection;
        }

        public void Dispose()
        {
            manager.Release(this);
        }

        internal LevelStorage(Guid guid, string path, LiteDatabase database, LevelStorageManager manager, IChunkParser chunkParser)
        {
            this.guid = guid;
            this.path = path;
            this.database = database;
            this.manager = manager;
            this.chunkParser = chunkParser;
            chunkCollection = new Lazy<ILiteCollection<BsonDocument>>(GetChunkCollection, true);
        }

        public readonly Guid guid;

        public readonly string path;

        private readonly LiteDatabase database;

        private readonly LevelStorageManager manager;

        private readonly IChunkParser chunkParser;

        private readonly Lazy<ILiteCollection<BsonDocument>> chunkCollection;

        private readonly Lazy<ILiteCollection<BsonDocument>> staticAnchorDataCollection;
    }
}
