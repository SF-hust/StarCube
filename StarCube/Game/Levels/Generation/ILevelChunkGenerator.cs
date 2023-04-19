using StarCube.Utility.Math;
using StarCube.Game.Levels.Chunks;

namespace StarCube.Game.Levels.Generation
{
    public interface ILevelChunkGenerator
    {
        public Chunk GenerateChunk(ChunkPos pos);
    }
}
