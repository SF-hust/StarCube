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
        /// Chunk 是否为空
        /// </summary>
        public bool IsEmpty { get; }

        /// <summary>
        /// 如果 Chunk 不可写, 什么也不做
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="blockState"></param>
        public void SetBlockState(int x, int y, int z, BlockState blockState);

        /// <summary>
        /// 如果 Chunk 不可写, 什么也不做
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="blockState"></param>
        public void SetBlockState(BlockPos pos, BlockState blockState)
        {
            SetBlockState(pos.x, pos.y, pos.z, blockState);
        }

        /// <summary>
        /// 获取 Chunk 中的某个 BlockState, 坐标为 Chunk 本地坐标
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public BlockState GetBlockState(int x, int y, int z);

        /// <summary>
        /// 获取 Chunk 中的某个 BlockState, 坐标为 Chunk 本地坐标
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public BlockState GetBlockState(BlockPos pos)
        {
            return GetBlockState(pos.x, pos.y, pos.z);
        }

        /// <summary>
        /// 将 Chunk 中的 BlockState 数据存储到一个 BlockState 数组中
        /// </summary>
        /// <param name="array"></param>
        public void CopyToArray(BlockState[] array);

        /// <summary>
        /// 将 Chunk 中的 BlockState 的下标数据存储到一个 int 数组中
        /// </summary>
        /// <param name="array"></param>
        public void CopyToArray(int[] array);
    }
}
