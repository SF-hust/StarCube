using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using StarCube.Utility.Math;
using StarCube.Utility.Container;
using StarCube.Game.Blocks;
using StarCube.Game.BlockEntities;

namespace StarCube.Game.Levels.Chunks
{
    public sealed class PalettedChunk : Chunk
    {
        public override bool Writable => true;

        public override bool Empty => blockStates.Level == 0;

        public override BlockState GetBlockState(int x, int y, int z)
        {
            int index = BlockPos.InChunkPosToIndex(x, y, z);
            return globalBlockStateMap.ValueFor(blockStates.Get(index));
        }

        public override void SetBlockState(int x, int y, int z, BlockState blockState)
        {
            int index = BlockPos.InChunkPosToIndex(x, y, z);
            blockStates.Set(index, blockState.IntegerID, pool);
            Modify = true;
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
                        blockStates.Set(index, blockStateIndex, pool);
                    }
                }
            }
            Modify = true;
        }

        public override BlockState GetAndSetBlockState(int x, int y, int z, BlockState blockState)
        {
            int index = BlockPos.InChunkPosToIndex(x, y, z);
            BlockState old = globalBlockStateMap.ValueFor(blockStates.Get(index));
            blockStates.Set(index, blockState.IntegerID, pool);
            Modify = true;
            return old;
        }

        public override void CopyBlockStatesTo(Span<BlockState> blockStates)
        {
            Debug.Assert(blockStates.Length == ChunkSize);

            for(int i = 0; i < blockStates.Length; ++i)
            {
                blockStates[i] = globalBlockStateMap.ValueFor(this.blockStates.Get(i));
            }
        }

        public override void CopyBlockStatesTo(Span<int> blockStates)
        {
            this.blockStates.CopyTo(blockStates);
        }

        public override Chunk CloneBlockStates()
        {
            return new PalettedChunk(Position, globalBlockStateMap, pool, blockStates.Clone(pool));
        }

        public void CompressPalettedData()
        {
            blockStates.Compress(pool);
        }



        public override void Clear()
        {
            blockStates.Clear(0, pool);
            Modify = true;
        }

        public override bool TryGetBlockEntity(BlockPos pos, [NotNullWhen(true)] out BlockEntity? blockEntity)
        {
            int index = pos.InChunkIndex;
            return inChunkIndexToBlockEntity.TryGetValue(index, out blockEntity);
        }

        public override bool TryAddBlockEntity(BlockPos pos, BlockEntity blockEntity)
        {
            int index = pos.InChunkIndex;
            if (inChunkIndexToBlockEntity.TryAdd(index, blockEntity))
            {
                blockEntity.OnActive(true);
                Modify = true;
                return true;
            }

            return false;
        }

        public override bool TryRemoveBlockEntity(BlockPos pos, [NotNullWhen(true)] out BlockEntity? blockEntity)
        {
            int index = pos.InChunkIndex;
            if (inChunkIndexToBlockEntity.TryGetValue(index, out blockEntity))
            {
                blockEntity.OnActive(false);
                inChunkIndexToBlockEntity.Remove(index);
                return true;
            }

            return false;
        }


        public override void OnActive(Level level, bool active)
        {
            foreach (BlockEntity blockEntity in inChunkIndexToBlockEntity.Values)
            {
                blockEntity.OnActive(active);
            }
        }

        public PalettedChunk(ChunkPos pos, IIDMap<BlockState> globalBlockStateMap, PalettedChunkDataPool pool)
            : base(pos)
        {
            this.globalBlockStateMap = globalBlockStateMap;
            blockStates = new PalettedChunkData(globalBlockStateMap.Count);
            this.pool = pool;
        }

        public PalettedChunk(ChunkPos pos, IIDMap<BlockState> globalBlockStateMap, PalettedChunkDataPool pool, int fillBlockState)
            : this(pos, globalBlockStateMap, pool)
        {
            blockStates.Clear(fillBlockState, pool);
        }

        public PalettedChunk(ChunkPos pos, IIDMap<BlockState> globalBlockStateMap, PalettedChunkDataPool pool, BlockState fillBlockState)
            : this(pos, globalBlockStateMap, pool, fillBlockState.IntegerID)
        {
        }

        public PalettedChunk(ChunkPos pos, IIDMap<BlockState> globalBlockStateMap, PalettedChunkDataPool pool, ReadOnlySpan<int> blockStates)
            : this(pos, globalBlockStateMap, pool)
        {
            this.blockStates.CopyFrom(blockStates, pool);
        }

        public PalettedChunk(ChunkPos pos, IIDMap<BlockState> globalBlockStateMap, PalettedChunkDataPool pool, ReadOnlySpan<BlockState> blockStates)
            : this(pos, globalBlockStateMap, pool)
        {
            for (int i = 0; i < ChunkSize; i++)
            {
                this.blockStates.Set(i, blockStates[i].IntegerID, pool);
            }
        }

        private PalettedChunk(ChunkPos pos, IIDMap<BlockState> globalBlockStateMap, PalettedChunkDataPool pool, PalettedChunkData blockStates)
            : base(pos)
        {
            this.pool = pool;
            this.globalBlockStateMap = globalBlockStateMap;
            this.blockStates = blockStates;
        }

        private readonly PalettedChunkDataPool pool;

        private readonly IIDMap<BlockState> globalBlockStateMap;

        private readonly PalettedChunkData blockStates;

        private readonly Dictionary<int, BlockEntity> inChunkIndexToBlockEntity = new Dictionary<int, BlockEntity>();
    }
}
