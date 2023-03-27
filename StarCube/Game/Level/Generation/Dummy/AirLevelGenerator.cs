using StarCube.Utility.Math;
using StarCube.Game.Level.Chunks;

namespace StarCube.Game.Level.Generation.Dummy
{
    public class AirLevelGenerator : ILevelGenerator
    {
        public Chunk GenerateChunk(ChunkPos pos)
        {
            return new EmptyChunk(pos);
        }
    }
}
