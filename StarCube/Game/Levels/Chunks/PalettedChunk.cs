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
            blockStates.Set(index, blockState.IntegerID, factory.pool);
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
                        blockStates.Set(index, blockStateIndex, factory.pool);
                    }
                }
            }
            Modify = true;
        }

        public override BlockState GetAndSetBlockState(int x, int y, int z, BlockState blockState)
        {
            int index = BlockPos.InChunkPosToIndex(x, y, z);
            BlockState old = globalBlockStateMap.ValueFor(blockStates.Get(index));
            blockStates.Set(index, blockState.IntegerID, factory.pool);
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
            return new PalettedChunk(Position, globalBlockStateMap, factory, blockStates.Clone(factory.pool));
        }

        public void CompressPalettedData()
        {
            blockStates.Compress(factory.pool);
        }



        public override void Clear()
        {
            blockStates.Clear(0, factory.pool);
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

        public override void Release()
        {
            factory.Release(this);
        }

        public override void Fill(BlockState blockState)
        {
            blockStates.Clear(blockState.IntegerID, factory.pool);
        }

        public void CopyBlockStatesFrom(ReadOnlySpan<int> blockStates)
        {
            this.blockStates.CopyFrom(blockStates, factory.pool);
        }

        public void CopyBlockStatesFrom(ReadOnlySpan<BlockState> blockStates)
        {
            for (int i = 0; i < ChunkSize; i++)
            {
                this.blockStates.Set(i, blockStates[i].IntegerID, factory.pool);
            }
        }

        internal PalettedChunk(ChunkPos pos, IIDMap<BlockState> globalBlockStateMap, PalettedChunkFactory factory)
            : base(pos)
        {
            this.globalBlockStateMap = globalBlockStateMap;
            blockStates = new PalettedChunkData(globalBlockStateMap.Count);
            this.factory = factory;
        }

        private PalettedChunk(ChunkPos pos, IIDMap<BlockState> globalBlockStateMap, PalettedChunkFactory factory, PalettedChunkData blockStates)
            : base(pos)
        {
            this.factory = factory;
            this.globalBlockStateMap = globalBlockStateMap;
            this.blockStates = blockStates;
        }

        private readonly PalettedChunkFactory factory;

        private readonly IIDMap<BlockState> globalBlockStateMap;

        private readonly PalettedChunkData blockStates;

        private readonly Dictionary<int, BlockEntity> inChunkIndexToBlockEntity = new Dictionary<int, BlockEntity>();
    }
}
