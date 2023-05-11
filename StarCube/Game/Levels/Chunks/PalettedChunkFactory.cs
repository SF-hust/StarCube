using System;
using System.Collections.Concurrent;

using StarCube.Utility.Math;
using StarCube.Utility.Container;
using StarCube.Game.Blocks;

namespace StarCube.Game.Levels.Chunks
{
    public sealed class PalettedChunkFactory : IChunkFactory
    {
        public Chunk CreateEmpty(ChunkPos pos)
        {
            return Get(pos);
        }

        public Chunk CreateWithFill(ChunkPos pos, int blockState)
        {
            PalettedChunk chunk = Get(pos);
            chunk.Fill(globalBlockStateIDMap.ValueFor(blockState));
            return chunk;
        }

        public Chunk CreateWithFill(ChunkPos pos, BlockState blockState)
        {
            PalettedChunk chunk = Get(pos);
            chunk.Fill(blockState);
            return chunk;
        }

        public Chunk Create(ChunkPos pos, ReadOnlySpan<int> blockStates)
        {
            PalettedChunk chunk = Get(pos);
            chunk.CopyBlockStatesFrom(blockStates);
            return chunk;
        }

        public Chunk Create(ChunkPos pos, ReadOnlySpan<BlockState> blockStates)
        {
            PalettedChunk chunk = Get(pos);
            chunk.CopyBlockStatesFrom(blockStates);
            return chunk;
        }

        private PalettedChunk Get(ChunkPos pos)
        {
            if (chunkPool.TryTake(out var chunk))
            {
                chunk.Reset(pos);
                return chunk;
            }

            return new PalettedChunk(pos, globalBlockStateIDMap, this);
        }

        internal void Release(PalettedChunk chunk)
        {
            chunk.Clear();
            if (chunkPool.Count < poolSize)
            {
                chunkPool.Add(chunk);
            }
        }

        public PalettedChunkFactory(IIDMap<BlockState> globalBlockStateIDMap, int poolSize = 4096)
        {
            this.globalBlockStateIDMap = globalBlockStateIDMap;
            pool = new PalettedChunkDataPool(16, 16, 4096, 4096, 1024, 256, 4096);
            this.poolSize = poolSize;
        }

        private readonly IIDMap<BlockState> globalBlockStateIDMap;

        public readonly PalettedChunkDataPool pool;

        private readonly int poolSize;

        private readonly ConcurrentBag<PalettedChunk> chunkPool = new ConcurrentBag<PalettedChunk>();
    }
}
