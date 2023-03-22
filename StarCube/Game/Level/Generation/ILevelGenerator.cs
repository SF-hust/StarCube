using StarCube.Utility.Math;
using StarCube.Game.Level.Chunk;

namespace StarCube.Game.Level.Generation
{
    public interface ILevelGenerator
    {
        public LevelChunk GenerateChunk(ChunkPos pos);
    }
}
