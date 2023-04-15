using System;

using LiteDB;

using StarCube.Utility.Math;
using StarCube.Utility.Container;
using StarCube.Game.Blocks;

namespace StarCube.Game.Levels.Chunks
{
    public class PalettedChunk : Chunk
    {
        public override bool Writable => true;

        public override bool Empty => blockStates.Level == 0;

        public override BlockState GetBlockState(int x, int y, int z)
        {
            int index = BlockPos.InChunkPosToIndex(x, y, z);
            return blockStates.Get(index);
        }

        public override void SetBlockState(int x, int y, int z, BlockState blockState)
        {
            int index = BlockPos.InChunkPosToIndex(x, y, z);
            blockStates.Set(index, blockState);
        }

        public override void FillBlockState(int x0, int y0, int z0, int x1, int y1, int z1, BlockState blockState)
        {
            int blockStateIndex = blockState.IntegerID;
            for (int y = y0; y < y1 + 1; ++y)
            {
                for (int z = z0; z < z1 + 1; ++z)
                {
                    for (int x = x0; x < x1 + 1; ++x)
                    {
                        int index = BlockPos.InChunkPosToIndex(x, y, z);
                        blockStates.SetRaw(index, blockStateIndex);
                    }
                }
            }
        }

        public override BlockState GetAndSetBlockState(int x, int y, int z, BlockState blockState)
        {
            int index = BlockPos.InChunkPosToIndex(x, y, z);
            BlockState old = blockStates.Get(index);
            blockStates.Set(index, blockState);
            return old;
        }

        public override void CopyBlockStatesTo(BlockState[] blockStates)
        {
            this.blockStates.CopyTo(blockStates);
        }

        public override void CopyBlockStatesTo(Span<int> buffer)
        {
            blockStates.CopyRawTo(buffer);
        }

        public override void StoreTo(BsonDocument bson)
        {
            throw new NotImplementedException();
        }

        public override void Clear()
        {
            blockStates.Clear();
        }

        public override Chunk Clone()
        {
            PalettedChunkData<BlockState> blockStates = this.blockStates.Clone();
            PalettedChunk clone = new PalettedChunk(pos, blockStates);
            return clone;
        }

        public PalettedChunkDataView GetReadOnlyBlockStateDataView()
        {
            return blockStates.AsReadOnlyView();
        }

        public PalettedChunk(ChunkPos pos, IIDMap<BlockState> globalBlockStateMap, PalettedChunkDataPool pool)
            : base(pos)
        {
            blockStates = new PalettedChunkData<BlockState>(globalBlockStateMap, pool);
        }

        public PalettedChunk(ChunkPos pos, IIDMap<BlockState> globalBlockStateMap, PalettedChunkDataPool pool, int fillBlockState)
            : this(pos, globalBlockStateMap, pool)
        {
            blockStates.Clear(fillBlockState);
        }

        public PalettedChunk(ChunkPos pos, IIDMap<BlockState> globalBlockStateMap, PalettedChunkDataPool pool, BlockState fillBlockState)
            : this(pos, globalBlockStateMap, pool, fillBlockState.IntegerID)
        {
        }

        public PalettedChunk(ChunkPos pos, IIDMap<BlockState> globalBlockStateMap, PalettedChunkDataPool pool, ReadOnlySpan<int> blockStates)
            : this(pos, globalBlockStateMap, pool)
        {
            this.blockStates.CopyRawFrom(blockStates);
        }

        public PalettedChunk(ChunkPos pos, IIDMap<BlockState> globalBlockStateMap, PalettedChunkDataPool pool, ReadOnlySpan<BlockState> blockStates)
            : this(pos, globalBlockStateMap, pool)
        {
            this.blockStates.CopyFrom(blockStates);
        }

        private PalettedChunk(ChunkPos pos, PalettedChunkData<BlockState> blockStates)
            : base(pos)
        {
            this.blockStates = blockStates;
        }

        private readonly PalettedChunkData<BlockState> blockStates;
    }
}
