using System;

using StarCube.Game.Block;
using StarCube.Utility.Math;

namespace StarCube.Game.Level.Chunk
{
    public sealed class EmptyChunk : LevelChunk
    {
        private static BlockState AirBlockState => Blocks.Air.StateDefinition.defaultState;

        public static EmptyChunk Create(WorldLevel level, ChunkPos pos)
        {
            return new EmptyChunk(level, pos);
        }

        public override bool Writable => false;

        public override bool Empty => true;

        public override BlockState GetAndSetBlockState(int x, int y, int z, BlockState blockState)
        {
            return AirBlockState;
        }

        public override BlockState GetBlockState(int x, int y, int z)
        {
            return AirBlockState;
        }

        public override void SetBlockState(int x, int y, int z, BlockState blockState)
        {
        }

        public override void FillBlockState(int x0, int y0, int z0, int x1, int y1, int z1, BlockState blockState)
        {
        }

        public override void CopyTo(BlockState[] array)
        {
            Array.Fill(array, AirBlockState);
        }

        public override void CopyTo(Span<int> array)
        {
            array.Fill(AirBlockState.IntegerID);
        }

        public EmptyChunk(WorldLevel level, ChunkPos pos)
            : base(level, pos)
        {
        }
    }
}
