using System;

using StarCube.Utility.Math;
using StarCube.Game.Level.Chunk;
using StarCube.Game.Block;

namespace StarCube.Game.Level.Generation.Flat
{
    public class FlatLevelGenerator : ILevelGenerator
    {
        public LevelChunk GenerateChunk(ChunkPos pos)
        {
            LevelChunk chunk = emptyChunkFactory(level, pos);
            for (int y = 0; y < 16; ++y)
            {
                BlockState blockState = layerList.GetBlockStateForHeight(y + pos.y * 16);
                chunk.FillBlockState(0, y, 0, 15, y + 1, 15, blockState);
            }

            return chunk;
        }

        public FlatLevelGenerator(WorldLevel level, Func<WorldLevel, ChunkPos, LevelChunk> emptyChunkFactory, FlatLayerList layerList)
        {
            this.level = level;
            this.emptyChunkFactory = emptyChunkFactory;
            this.layerList = layerList;
        }

        private readonly WorldLevel level;
        private readonly Func<WorldLevel, ChunkPos, LevelChunk> emptyChunkFactory;
        private readonly FlatLayerList layerList;
    }
}
