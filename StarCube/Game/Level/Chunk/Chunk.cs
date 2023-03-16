using StarCube.Utility.Math;
using StarCube.Utility.Container;
using StarCube.Game.Block;

namespace StarCube.Game.Level.Chunk
{
    public class Chunk : IChunk
    {
        public static int IndexOfPos(int x, int y, int z)
        {
            return x + (y << 8) + (z << 4);
        }

        public ILevel Level => level;

        public ChunkPos ChunkPos => chunkPos;

        public bool Writable => true;

        public bool IsEmpty => false;

        public void CopyToArray(int[] array)
        {
            blockStates.data.CopyTo(array, 0);
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
            return blockStates[IndexOfPos(x, y, z)];
        }

        public void SetBlockState(int x, int y, int z, BlockState blockState)
        {
            blockStates[IndexOfPos(x, y, z)] = blockState;
        }

        public BlockState GetAndSetBlockState(int x, int y, int z, BlockState blockState)
        {
            int i = IndexOfPos(x, y, z);
            BlockState oldState = blockStates[i];
            blockStates[i] = blockState;
            return oldState;
        }

        public Chunk(Level level, ChunkPos chunkPos, IIDMap<BlockState> globalBlockStateMap, BlockState initState)
        {
            this.level = level;
            this.chunkPos = chunkPos;
            blockStates = new GlobalPalettedContainer<BlockState>(globalBlockStateMap, 4096);
        }

        private readonly Level level;
        private readonly ChunkPos chunkPos;
        private readonly GlobalPalettedContainer<BlockState> blockStates;
    }
}
