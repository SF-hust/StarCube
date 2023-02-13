using System;
using System.Diagnostics.CodeAnalysis;

using StarCube.Utility.Math;
using StarCube.Game.Block;
using StarCube.Game.Level.Chunk;

namespace StarCube.Game.Level
{
    public abstract class Level : ILevel
    {
        public bool TryGetBlockState(int x, int y, int z, [NotNullWhen(true)] out BlockState? blockState)
        {
            throw new NotImplementedException();
        }

        public bool TryGetBlockState(BlockPos pos, [NotNullWhen(true)] out BlockState? blockState)
        {
            throw new NotImplementedException();
        }

        public bool TryGetChunk(int x, int y, int z, [NotNullWhen(true)] out IChunk? chunk)
        {
            throw new NotImplementedException();
        }

        public bool TryGetChunk(ChunkPos pos, [NotNullWhen(true)] out IChunk? chunk)
        {
            throw new NotImplementedException();
        }

        public bool TrySetBlockState(int x, int y, int z, BlockState blockState)
        {
            throw new NotImplementedException();
        }

        public bool TrySetBlockState(BlockPos pos, BlockState blockState)
        {
            throw new NotImplementedException();
        }
    }
}
