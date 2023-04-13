﻿using StarCube.Utility.Math;
using StarCube.Game.Levels.Chunks;

namespace StarCube.Game.Levels.Generation.Dummy
{
    public class AirLevelGenerator : ILevelGenerator
    {
        public Chunk GenerateChunk(ChunkPos pos)
        {
            return chunkFactory.CreateEmpty(pos);
        }

        public AirLevelGenerator(IChunkFactory chunkFactory)
        {
            this.chunkFactory = chunkFactory;
        }

        private readonly IChunkFactory chunkFactory;
    }
}
