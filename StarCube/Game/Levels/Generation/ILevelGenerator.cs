using StarCube.Utility.Math;
using StarCube.Game.Levels.Chunks;

namespace StarCube.Game.Levels.Generation
{
    public interface ILevelGenerator
    {
        public Chunk GenerateChunk(ChunkPos pos);
    }
}
