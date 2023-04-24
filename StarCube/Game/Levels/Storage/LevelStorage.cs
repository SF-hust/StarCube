using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using System;

using LiteDB;

using StarCube.Utility;
using StarCube.Utility.Math;
using StarCube.Data.Storage;
using StarCube.Game.Levels.Chunks;
using StarCube.Game.Levels.Chunks.Loading;
using StarCube.Utility.Logging;

namespace StarCube.Game.Levels.Storage
{
    /// <summary>
    /// 表示一个 Level 的数据存储
    /// </summary>
    public sealed class LevelStorage
    {
        public const string ChunkCollectionName = "chunk";

        public const string StaticAnchorCollectionName = "static_anchor";

        public bool Contains(ChunkPos pos)
        {
            if (!database.Created)
            {
                return false;
            }
            return chunkCollection.Value.Exists(Query.EQ("_id", pos.ToObjectID()));
        }

        public bool TryLoadChunk(ChunkPos pos, [NotNullWhen(true)] out Chunk? chunk)
        {
            chunk = null;
            if (!database.Created)
            {
                return false;
            }
            BsonDocument? bson = chunkCollection.Value.FindById(pos.ToObjectID());
            if (bson == null)
            {
                return false;
            }
            return manager.chunkParser.TryParse(bson, pos, out chunk);
        }

        public void SaveChunk(Chunk chunk)
        {
            BsonDocument bson = manager.chunkParser.ToBson(chunk);
            chunkCollection.Value.Upsert(chunk.pos.ToObjectID(), bson);
        }

        public void LoadStaticLevelAnchors(out long nextID, out Dictionary<long, AnchorData> idToStaticAnchor)
        {
            idToStaticAnchor = new Dictionary<long, AnchorData>();
            if (!database.Created)
            {
                nextID = 1L;
                idToStaticAnchor = new Dictionary<long, AnchorData>();
                return;
            }

            // 读取下一个静态加载锚的 id
            BsonDocument? currentIDDoc = staticAnchorCollection.Value.FindById(0L);
            if (currentIDDoc == null)
            {
                currentIDDoc = new BsonDocument();
                currentIDDoc["next"] = 1L;
            }
            nextID = currentIDDoc["next"].AsInt64;

            // 读取所有静态加载锚数据
            foreach (BsonDocument anchorDoc in staticAnchorCollection.Value.Find(Query.GT("_id", 0L)))
            {
                if (anchorDoc.TryGetChunkPos("pos", out ChunkPos pos) && anchorDoc.TryGetInt32("radius", out int radius))
                {
                    idToStaticAnchor.Add(anchorDoc["_id"].AsInt64, new AnchorData(pos, radius));
                }
            }
        }

        public void WriteStaticLevelAnchors(long currentID, Dictionary<long, AnchorData> idToAnchorData)
        {
            staticAnchorCollection.Value.DeleteAll();
            BsonDocument currentIDDoc = new BsonDocument();
            currentIDDoc["next"] = currentID;
            staticAnchorCollection.Value.Insert(0L, currentIDDoc);
            foreach (var pair in idToAnchorData)
            {
                BsonDocument anchorDoc = new BsonDocument();
                anchorDoc.Add("pos", pair.Value.chunkPos);
                anchorDoc.Add("radius", pair.Value.radius);
            }
        }

        public void Dispose()
        {
            if (disposed)
            {
                LogUtil.Error("LevelStorage disposed");
                throw new ObjectDisposedException(nameof(LevelStorage));
            }
            manager.Release(this);
            disposed = true;
        }

        internal LevelStorage(Guid guid, LevelStorageManager manager, StorageDatabase database)
        {
            this.guid = guid;
            this.manager = manager;
            this.database = database;
            chunkCollection = new Lazy<ILiteCollection<BsonDocument>>(() => database.Value.GetCollectionAndEnsureIndex(ChunkCollectionName, BsonAutoId.ObjectId));
            staticAnchorCollection = new Lazy<ILiteCollection<BsonDocument>>(() => database.Value.GetCollectionAndEnsureIndex(StaticAnchorCollectionName, BsonAutoId.Int64));
        }

        public readonly Guid guid;

        private readonly LevelStorageManager manager;

        public readonly StorageDatabase database;

        private readonly Lazy<ILiteCollection<BsonDocument>> chunkCollection;

        private readonly Lazy<ILiteCollection<BsonDocument>> staticAnchorCollection;

        private volatile bool disposed = false;
    }
}
