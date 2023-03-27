using System;

using StarCube.Utility.Math;
using StarCube.Utility.Container;
using StarCube.Game.Block;

namespace StarCube.Game.Level.Chunks
{
    public sealed class PalettedChunk : Chunk
    {
        public override bool Writable => true;

        public override bool Empty => false;

        public override BlockState GetAndSetBlockState(int x, int y, int z, BlockState blockState)
        {
            int index = BlockPos.InChunkPosToIndex(x, y, z);
            BlockState old = globalBlockStateMap.ValueFor(data[index]);
            data[index] = globalBlockStateMap.IdFor(blockState);
            return old;
        }

        public override BlockState GetBlockState(int x, int y, int z)
        {
            int index = BlockPos.InChunkPosToIndex(x, y, z);
            return globalBlockStateMap.ValueFor(data[index]);
        }

        public override void SetBlockState(int x, int y, int z, BlockState blockState)
        {
            int index = BlockPos.InChunkPosToIndex(x, y, z);
            data[index] = globalBlockStateMap.IdFor(blockState);
        }

        public override void FillBlockState(int x0, int y0, int z0, int x1, int y1, int z1, BlockState blockState)
        {
            int blockStateIndex = globalBlockStateMap.IdFor(blockState);

            for(int y = y0; y < y1 + 1; ++y)
            {
                for(int z = z0; z < z1 + 1; ++z)
                {
                    for(int x = x0; x < x1 + 1; ++x)
                    {
                        int index = BlockPos.InChunkPosToIndex(x, y, z);
                        data[index] = blockStateIndex;
                    }
                }
            }
        }

        public override void CopyTo(BlockState[] array)
        {
            for (int i = 0; i < 4096; ++i)
            {
                array[i] = globalBlockStateMap.ValueFor(data[i]);
            }
        }

        public override void CopyTo(Span<int> array)
        {
            data.CopyTo(array);
        }

        public PalettedChunk(ChunkPos pos, IIDMap<BlockState> globalBlockStateMap, BlockState initState)
            : base(pos)
        {
            this.globalBlockStateMap = globalBlockStateMap;
            data = new int[4096];
            Array.Fill(data, globalBlockStateMap.IdFor(initState));
        }

        private readonly IIDMap<BlockState> globalBlockStateMap;

        private readonly int[] data;
    }
}
