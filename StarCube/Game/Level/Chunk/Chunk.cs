using StarCube.Utility.Math;
using StarCube.Utility.Container;
using StarCube.Game.Block;

namespace StarCube.Game.Level.Chunk
{
    public class Chunk : IChunk
    {
        public ILevel Level => level;

        public ChunkPos ChunkPos => chunkPos;

        public bool Writable => true;

        public bool IsEmpty => false;

        private readonly Level level;
        private readonly ChunkPos chunkPos;
        private readonly IPalettedContainer<BlockState> blockStates;

        public Chunk(Level level, ChunkPos chunkPos, BlockState initState)
        {
            this.level = level;
            this.chunkPos = chunkPos;
            blockStates = new NonPalettedContainer<BlockState>(4096, initState);
        }

        public void CopyToArray(int[] array)
        {
        }
        public void CopyToArray(BlockState[] array)
        {
            for(int i = 0; i < array.Length; i++)
            {
                array[i] = blockStates[i];
            }
        }

        public BlockState GetBlockState(int x, int y, int z)
        {
            return blockStates[x + (y << 8) + (z << 4)];
        }

        public void SetBlockState(int x, int y, int z, BlockState blockState)
        {
            blockStates[x + (y << 8) + (z << 4)] = blockState;
        }
    }
}
