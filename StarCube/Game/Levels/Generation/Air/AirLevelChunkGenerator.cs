using StarCube.Utility.Math;
using StarCube.Game.Levels.Chunks;

namespace StarCube.Game.Levels.Generation.Air
{
    public class AirLevelChunkGenerator : ILevelChunkGenerator
    {
        public Chunk GenerateChunk(ChunkPos pos)
        {
            return chunkFactory.CreateEmpty(pos);
        }

        public AirLevelChunkGenerator(IChunkFactory chunkFactory)
        {
            this.chunkFactory = chunkFactory;
        }

        private readonly IChunkFactory chunkFactory;
    }
}
