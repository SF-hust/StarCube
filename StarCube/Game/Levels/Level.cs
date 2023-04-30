using System;
using System.Diagnostics.CodeAnalysis;

using StarCube.Utility;
using StarCube.Utility.Math;
using StarCube.Game.Blocks;
using StarCube.Game.Levels.Chunks;
using StarCube.Game.Worlds;
using System.Numerics;

namespace StarCube.Game.Levels
{
    public abstract class Level :
        IGuid
    {
        public Vector3 Position
        {
            get => position;
            set => position = value;
        }

        public Quaternion Rotation
        {
            get => rotation;
            set => rotation = value;
        }

        public abstract bool Active { get; set; }

        public abstract World World { get; }

        public abstract bool HasBlock(int x, int y, int z);
        public abstract bool HasBlock(BlockPos pos);

        public abstract bool TryGetBlockState(int x, int y, int z, [NotNullWhen(true)] out BlockState? blockState);
        public abstract bool TryGetBlockState(BlockPos pos, [NotNullWhen(true)] out BlockState? blockState);

        public abstract bool TrySetBlockState(int x, int y, int z, BlockState blockState);
        public abstract bool TrySetBlockState(BlockPos pos, BlockState blockState);

        public abstract bool TryGetAndSetBlockState(BlockPos pos, BlockState blockState, [NotNullWhen(true)] out BlockState? oldBlockState);
        public abstract bool TryGetAndSetBlockState(int x, int y, int z, BlockState blockState, [NotNullWhen(true)] out BlockState? oldBlockState);

        public abstract bool HasChunk(ChunkPos pos);

        public abstract bool TryGetChunk(ChunkPos pos, [NotNullWhen(true)] out Chunk? chunk);

        public abstract void Tick();


        Guid IGuid.Guid => guid;

        public Level(Guid guid, ILevelBounding bounding)
        {
            this.guid = guid;
            this.bounding = bounding;
        }

        public readonly Guid guid;

        public readonly ILevelBounding bounding;

        private Vector3 position = Vector3.Zero;
        private Quaternion rotation = Quaternion.Identity;
    }
}
