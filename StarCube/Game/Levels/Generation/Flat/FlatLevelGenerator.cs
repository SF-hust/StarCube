using System;

using StarCube.Utility.Math;
using StarCube.Game.Levels.Chunks;
using StarCube.Game.Blocks;

namespace StarCube.Game.Levels.Generation.Flat
{
    public class FlatLevelGenerator : ILevelGenerator
    {
        public Chunk GenerateChunk(ChunkPos pos)
        {
            Span<int> buffer = stackalloc int[Chunk.ChunkSize];
            for (int y = 0; y < 16; ++y)
            {
                int id = layerList.GetBlockStateIDForHeight(y + pos.y * 16);
                buffer.Slice(y * 256, 256).Fill(id);
            }
            Chunk chunk = chunkFactory.Create(pos, buffer);
            return chunk;
        }

        public FlatLevelGenerator(IChunkFactory chunkFactory, FlatLayerList layerList)
        {
            this.chunkFactory = chunkFactory;
            this.layerList = layerList;
        }

        private readonly IChunkFactory chunkFactory;
        private readonly FlatLayerList layerList;
    }
}
