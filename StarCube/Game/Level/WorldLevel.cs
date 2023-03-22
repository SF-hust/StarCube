using System;
using System.Diagnostics.CodeAnalysis;

using StarCube.Utility;
using StarCube.Utility.Math;
using StarCube.Game.Block;
using StarCube.Game.Level.Chunk;
using StarCube.Game.Ticking;

namespace StarCube.Game.Level
{
    public abstract class WorldLevel :
        ITickable,
        IGuid
    {
        public abstract bool TryGetBlockState(int x, int y, int z, [NotNullWhen(true)] out BlockState? blockState);

        public abstract bool TryGetBlockState(BlockPos pos, [NotNullWhen(true)] out BlockState? blockState);

        public abstract bool TrySetBlockState(int x, int y, int z, BlockState blockState);

        public abstract bool TrySetBlockState(BlockPos pos, BlockState blockState);

        public abstract bool TryGetAndSetBlockState(BlockPos pos, BlockState blockState, [NotNullWhen(true)] out BlockState? oldBlockState);

        public abstract bool TryGetAndSetBlockState(int x, int y, int z, BlockState blockState, [NotNullWhen(true)] out BlockState? oldBlockState);

        public abstract bool TryGetChunk(int x, int y, int z, [NotNullWhen(true)] out LevelChunk? chunk);

        public abstract bool TryGetChunk(ChunkPos pos, [NotNullWhen(true)] out LevelChunk? chunk);

        public virtual void Tick()
        {
        }

        Guid IGuid.Guid => guid;

        public WorldLevel(Guid guid)
        {
            this.guid = guid;
        }

        public readonly Guid guid;
    }
}
