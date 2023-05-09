using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using System;

using LiteDB;

using StarCube.Utility;
using StarCube.Utility.Math;
using StarCube.Data.Storage;
using StarCube.Data.Storage.Exceptions;
using StarCube.Game.Levels.Chunks;
using StarCube.Game.Levels.Chunks.Loading;
using StarCube.Game.Levels.Chunks.Storage;
using System.Linq;

namespace StarCube.Game.Levels.Storage
{
    /// <summary>
    /// 表示一个 Level 的数据存储
    /// </summary>
    public sealed class LevelStorage
    {
        public const string ChunkCollectionName = "chunk";

        public const string StaticAnchorCollectionName = "static_anchor";

        public const string StaticAnchorNextIndexField = "next";

        public const string StaticAnchorPosField = "pos";

        public const string StaticAnchorRadiusField = "radius";


        public bool Created => database.Created;

        public IChunkFactory ChunkFactory => manager.chunkFactory;

        public IChunkParser ChunkParser => manager.chunkParser;

        /// <summary>
        /// 检查数据库中是否已存在对应位置的 chunk
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public bool Contains(ChunkPos pos)
        {
            CheckReleased();

            if (!database.Created)
            {
                return false;
            }

            return chunkCollection.Value.Exists(Query.EQ("_id", pos.ToObjectID()));
        }

        /// <summary>
        /// 尝试从数据库中加载对应位置的 chunk
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="chunk"></param>
        /// <returns></returns>
        public bool TryLoadChunk(ChunkPos pos, [NotNullWhen(true)] out Chunk? chunk)
        {
            CheckReleased();

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
            return ChunkParser.TryParse(bson, pos, out chunk);
        }

        /// <summary>
        /// 从数据库中加载所有 chunk
        /// </summary>
        /// <returns></returns>
        /// <exception cref="GameSavesCorruptException"></exception>
        public IEnumerable<Chunk> LoadAllChunks()
        {
            if (!database.Created)
            {
                yield break;
            }

            foreach (BsonDocument bson in chunkCollection.Value.FindAll())
            {
                ChunkPos pos = bson["_id"].AsObjectId.ToChunkPos();
                if (!ChunkParser.TryParse(bson, pos, out Chunk? chunk))
                {
                    throw new GameSavesCorruptException("chunk parse failed");
                }

                yield return chunk;
            }
        }

        /// <summary>
        /// 将 chunk 保存到数据库中
        /// </summary>
        /// <param name="chunk"></param>
        public void SaveChunk(Chunk chunk)
        {
            CheckReleased();

            BsonDocument bson = ChunkParser.ToBson(chunk);
            chunkCollection.Value.Upsert(chunk.Position.ToObjectID(), bson);
        }

        /// <summary>
        /// 从数据库中加载静态锚点以及锚点的下一个 index
        /// </summary>
        /// <param name="nextIndex"></param>
        /// <param name="indexToStaticAnchor"></param>
        public void LoadStaticLevelAnchors(out long nextIndex, out Dictionary<long, AnchorData> indexToStaticAnchor)
        {
            CheckReleased();

            indexToStaticAnchor = new Dictionary<long, AnchorData>();
            if (!database.Created)
            {
                nextIndex = 1L;
                return;
            }

            // 读取下一个静态加载锚点的 index
            BsonDocument currentIDDoc = staticAnchorCollection.Value.FindById(0L) ?? throw new GameSavesCorruptException(nameof(nextIndex));
            nextIndex = currentIDDoc[StaticAnchorNextIndexField].AsInt64;

            // 读取所有静态加载锚数据
            foreach (BsonDocument anchorDocument in staticAnchorCollection.Value.Find(Query.Not("_id", 0L)))
            {
                if (anchorDocument.TryGetChunkPos(StaticAnchorPosField, out ChunkPos pos) && anchorDocument.TryGetInt32(StaticAnchorRadiusField, out int radius))
                {
                    indexToStaticAnchor.Add(anchorDocument["_id"].AsInt64, new AnchorData(pos, radius));
                    continue;
                }

                throw new GameSavesCorruptException(nameof(anchorDocument));
            }
        }

        /// <summary>
        /// 向数据库中写入修改过的静态锚点数据
        /// </summary>
        /// <param name="currentIndex"></param>
        /// <param name="indexToModifiedAnchorData"></param>
        public void WriteStaticLevelAnchors(long currentIndex, Dictionary<long, AnchorData> indexToModifiedAnchorData, IEnumerable<long> removedAnchorIndexes)
        {
            // 更新 index
            BsonDocument currentIDDoc = staticAnchorCollection.Value.FindById(0L) ?? new BsonDocument();
            currentIDDoc[StaticAnchorNextIndexField] = currentIndex;
            staticAnchorCollection.Value.Upsert(0L, currentIDDoc);

            // 更新已修改的锚点数据
            foreach (var pair in indexToModifiedAnchorData)
            {
                BsonDocument bson = new BsonDocument();
                bson[StaticAnchorPosField] = pair.Value.chunkPos.ToObjectID();
                bson[StaticAnchorRadiusField] = pair.Value.radius;
                staticAnchorCollection.Value.Upsert(pair.Key, bson);
            }

            // 删除被移除的锚点数据
            foreach (var index in removedAnchorIndexes)
            {
                staticAnchorCollection.Value.Delete(index);
            }
        }


        public void Release()
        {
            if (released)
            {
                throw new ObjectDisposedException(nameof(LevelStorage), "double release");
            }

            manager.Release(this);

            released = true;
        }

        private void CheckReleased()
        {
            if (released)
            {
                throw new ObjectDisposedException(nameof(LevelStorage), "released");
            }
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

        private volatile bool released = false;
    }
}
