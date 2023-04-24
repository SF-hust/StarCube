using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using StarCube.Utility.Math;
using StarCube.Game.Blocks;
using StarCube.Game.Levels.Chunks;
using StarCube.Game.Levels.Storage;
using StarCube.Game.Levels.Generation;
using StarCube.Game.Worlds;

namespace StarCube.Game.Levels
{
    public sealed class IntegralServerLevel : ServerLevel
    {
        public override bool HasBlock(int x, int y, int z)
        {
            return HasBlock(new BlockPos(x, y, z));
        }

        public override bool HasBlock(BlockPos pos)
        {
            ChunkPos chunkPos = pos.GetChunkPos();
            return posToChunk.ContainsKey(chunkPos);
        }

        public override bool TryGetBlockState(int x, int y, int z, [NotNullWhen(true)] out BlockState? blockState)
        {
            return TryGetBlockState(new BlockPos(x, y, z), out blockState);
        }

        public override bool TryGetBlockState(BlockPos pos, [NotNullWhen(true)] out BlockState? blockState)
        {
            ChunkPos chunkPos = pos.GetChunkPos();
            if (posToChunk.TryGetValue(chunkPos, out Chunk? chunk))
            {
                blockState = chunk.GetBlockState(pos.GetInChunkPos());
                return true;
            }

            blockState = null;
            return false;
        }

        public override bool TrySetBlockState(int x, int y, int z, BlockState blockState)
        {
            return TrySetBlockState(new BlockPos(x, y, z), blockState);
        }

        public override bool TrySetBlockState(BlockPos pos, BlockState blockState)
        {
            ChunkPos chunkPos = pos.GetChunkPos();
            if (posToChunk.TryGetValue(chunkPos, out Chunk? chunk))
            {
                chunk.SetBlockState(pos.GetInChunkPos(), blockState);
                return true;
            }

            return false;
        }

        public override bool TryGetAndSetBlockState(int x, int y, int z, BlockState blockState, [NotNullWhen(true)] out BlockState? oldBlockState)
        {
            return TryGetAndSetBlockState(new BlockPos(x, y, z), blockState, out oldBlockState);
        }

        public override bool TryGetAndSetBlockState(BlockPos pos, BlockState blockState, [NotNullWhen(true)] out BlockState? oldBlockState)
        {
            ChunkPos chunkPos = pos.GetChunkPos();
            if (posToChunk.TryGetValue(chunkPos, out Chunk? chunk))
            {
                oldBlockState = chunk.GetAndSetBlockState(pos.GetInChunkPos(), blockState);
                return true;
            }

            oldBlockState = null;
            return false;
        }

        public override void Tick()
        {
            TickLevelUpdate();
        }

        public override bool HasChunk(ChunkPos pos)
        {
            return posToChunk.ContainsKey(pos);
        }

        public override bool TryGetChunk(ChunkPos pos, [NotNullWhen(true)] out Chunk? chunk)
        {
            return posToChunk.TryGetValue(pos, out chunk);
        }

        protected override void DoSave()
        {
            foreach (Chunk chunk in posToChunk.Values)
            {
                if (chunk.Modify)
                {
                    storage.SaveChunk(chunk);
                }
                chunk.Modify = false;
            }
        }

        public IntegralServerLevel(Guid guid, ILevelBounding bounding, ServerWorld world, ILevelChunkGenerator generator, LevelStorage storage)
            : base(guid, bounding, world, generator, storage)
        {

        }

        private readonly Dictionary<ChunkPos, Chunk> posToChunk = new Dictionary<ChunkPos, Chunk>();
    }
}
