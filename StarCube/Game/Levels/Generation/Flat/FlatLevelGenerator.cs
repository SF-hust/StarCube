using System;

using StarCube.Utility.Math;
using StarCube.Game.Levels.Chunks;
using StarCube.Game.Block;

namespace StarCube.Game.Levels.Generation.Flat
{
    public class FlatLevelGenerator : ILevelGenerator
    {
        public Chunk GenerateChunk(ChunkPos pos)
        {
            Chunk chunk = emptyChunkFactory(pos);
            for (int y = 0; y < 16; ++y)
            {
                BlockState blockState = layerList.GetBlockStateForHeight(y + pos.y * 16);
                chunk.FillBlockState(0, y, 0, 15, y, 15, blockState);
            }

            return chunk;
        }

        public FlatLevelGenerator(Func<ChunkPos, Chunk> emptyChunkFactory, FlatLayerList layerList)
        {
            this.emptyChunkFactory = emptyChunkFactory;
            this.layerList = layerList;
        }

        private readonly Func<ChunkPos, Chunk> emptyChunkFactory;
        private readonly FlatLayerList layerList;
    }
}
