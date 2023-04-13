using StarCube.Utility.Math;
using StarCube.Game.Blocks;

namespace StarCube.Game.Levels.Chunks
{
    public interface IChunkFactory
    {
        public Chunk CreateEmpty(ChunkPos pos);

        public Chunk Create(ChunkPos pos, int[] blockStates);

        public Chunk Create(ChunkPos pos, BlockState[] blockStates);
    }
}
