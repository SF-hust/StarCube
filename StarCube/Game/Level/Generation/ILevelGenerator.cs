using StarCube.Utility.Math;
using StarCube.Game.Level.Chunks;

namespace StarCube.Game.Level.Generation
{
    public interface ILevelGenerator
    {
        public Chunk GenerateChunk(ChunkPos pos);
    }
}
