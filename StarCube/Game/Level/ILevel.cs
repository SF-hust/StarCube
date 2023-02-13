using System;
using System.Diagnostics.CodeAnalysis;

using StarCube.Utility.Math;
using StarCube.Game.Block;
using StarCube.Game.Level.Chunk;

namespace StarCube.Game.Level
{
    /// <summary>
    /// 一个 Level 是一个由方块组成的，可以在游戏世界中展示的实体
    /// </summary>
    public interface ILevel
    {
        public bool TryGetBlockState(int x, int y, int z, [NotNullWhen(true)] out BlockState? blockState);

        public bool TryGetBlockState(BlockPos pos, [NotNullWhen(true)] out BlockState? blockState);

        public BlockState GetBlockState(int x, int y, int z)
        {
            return TryGetBlockState(x, y, z, out BlockState? blockState) ? blockState : throw new Exception();
        }

        public BlockState GetBlockState(BlockPos pos)
        {
            return TryGetBlockState(pos, out BlockState? blockState) ? blockState : throw new Exception();
        }

        public bool TrySetBlockState(int x, int y, int z, BlockState blockState);

        public bool TrySetBlockState(BlockPos pos, BlockState blockState);

        public void SetBlockState(int x, int y, int z, BlockState blockState)
        {
            if(!TrySetBlockState(x, y, z, blockState))
            {
                throw new Exception();
            }
        }

        public void SetBlockState(BlockPos pos, BlockState blockState)
        {
            if (!TrySetBlockState(pos, blockState))
            {
                throw new Exception();
            }
        }

        public bool TryGetChunk(int x, int y, int z, [NotNullWhen(true)] out IChunk? chunk);

        public bool TryGetChunk(ChunkPos pos, [NotNullWhen(true)] out IChunk? chunk);
    }
}
