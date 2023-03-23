using StarCube.Utility.Math;
using StarCube.Game.Level.Chunk;

namespace StarCube.Game.Level.Generation.Dummy
{
    public class AirLevelGenerator : ILevelGenerator
    {
        public LevelChunk GenerateChunk(ChunkPos pos)
        {
            return new EmptyChunk(pos);
        }
    }
}
