using StarCube.Utility.Math;
using StarCube.Game.Block;

namespace StarCube.Game.Level.Chunk
{
    public interface IChunk
    {
        /// <summary>
        /// Chunk 所属的 Level
        /// </summary>
        public ILevel Level { get; }

        /// <summary>
        /// Chunk 在所属 World 中的坐标
        /// </summary>
        public ChunkPos ChunkPos { get; }

        /// <summary>
        /// Chunk 在所属 World 中的X坐标
        /// </summary>
        public int X => ChunkPos.x;
        /// <summary>
        /// Chunk 在所属 World 中的Y坐标
        /// </summary>
        public int Y => ChunkPos.y;
        /// <summary>
        /// Chunk 在所属 World 中的Z坐标
        /// </summary>
        public int Z => ChunkPos.z;

        /// <summary>
        /// 可否更改 Chunk 的内容
        /// </summary>
        public bool Writable { get; }

        /// <summary>
        /// Chunk 是否为空，即所有 BlockState 均为空气 (index = 0)
        /// </summary>
        public bool IsEmpty { get; }

        public void SetBlockState(int x, int y, int z, BlockState blockState);

        public void SetBlockState(BlockPos pos, BlockState blockState)
        {
            SetBlockState(pos.x, pos.y, pos.z, blockState);
        }

        public BlockState GetAndSetBlockState(int x, int y, int z, BlockState blockState);

        public BlockState GetAndSetBlockState(BlockPos pos, BlockState blockState)
        {
            return GetAndSetBlockState(pos.x, pos.y, pos.z, blockState);
        }

        public BlockState GetBlockState(int x, int y, int z);

        public BlockState GetBlockState(BlockPos pos)
        {
            return GetBlockState(pos.x, pos.y, pos.z);
        }

        public void CopyToArray(BlockState[] array);

        public void CopyToArray(int[] array);
    }
}
