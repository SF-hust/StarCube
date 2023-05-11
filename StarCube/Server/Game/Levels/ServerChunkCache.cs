using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using StarCube.Utility.Math;
using StarCube.Game.Levels.Generation;
using StarCube.Game.Levels.Storage;
using StarCube.Game.Levels.Chunks;
using StarCube.Game.Levels.Chunks.Source;
using StarCube.Game.Levels.Chunks.Loading;

namespace StarCube.Server.Game.Levels
{
    public sealed class ServerChunkCache : IDisposable
    {
        public bool HasChunk(ChunkPos pos)
        {
            return chunkMap.IsLoaded(pos);
        }

        public bool TryGetChunk(ChunkPos pos, [NotNullWhen(true)] out Chunk? chunk)
        {
            return chunkMap.TryGet(pos, out chunk);
        }

        public bool TryAddAnchor(ChunkLoadAnchor anchor)
        {
            chunkMap.AddAnchor(anchor);
            return true;
        }

        public void RemoveAnchor(ChunkLoadAnchor anchor)
        {
            chunkMap.RemoveAnchor(anchor);
        }

        public void TickChunks()
        {
        }

        public void TickChunkSource()
        {
            chunkMap.Update();
        }

        public void FlushChunks()
        {
            chunkMap.Flush();
        }

        public void Dispose()
        {
            chunkMap.Dispose();
        }

        public void OnChunkModify(Chunk chunk)
        {
            posToPendingSaveChunk.TryAdd(chunk.Position, chunk);
        }

        public void PutUnloadChunk(Chunk chunk)
        {
            posToPendingUnloadChunk.Add(chunk.Position, chunk);
        }

        public bool TryRemoveChunkFromCache(ChunkPos pos, [NotNullWhen(true)] out Chunk? chunk)
        {
            return posToPendingUnloadChunk.Remove(pos, out chunk);
        }

        public void ClearCache()
        {
            foreach (Chunk chunk in posToPendingUnloadChunk.Values)
            {
                chunk.Release();
            }

            posToPendingUnloadChunk.Clear();
        }

        public void Save()
        {
            foreach (Chunk chunk in posToPendingSaveChunk.Values)
            {
                storage.SaveChunk(chunk);
            }

            foreach (Chunk chunk in posToPendingUnloadChunk.Values)
            {
                chunk.Release();
            }

            posToPendingSaveChunk.Clear();
            posToPendingUnloadChunk.Clear();
        }

        public ServerChunkCache(ChunkedServerLevel level, ILevelChunkGenerator generator, LevelStorage storage)
        {
            this.level = level;
            this.storage = storage;
            chunkMap = new ChunkMap(this, new ChunkProvider(generator, storage, 4096), level.bounding);
        }

        public readonly ChunkedServerLevel level;

        private readonly LevelStorage storage;

        private readonly Dictionary<ChunkPos, Chunk> posToPendingUnloadChunk = new Dictionary<ChunkPos, Chunk>();

        private readonly Dictionary<ChunkPos, Chunk> posToPendingSaveChunk = new Dictionary<ChunkPos, Chunk>();

        private readonly ChunkMap chunkMap;
    }
}
