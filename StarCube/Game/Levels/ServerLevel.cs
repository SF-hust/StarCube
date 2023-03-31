using System;
using System.Diagnostics.CodeAnalysis;

using StarCube.Utility.Math;
using StarCube.Game.Blocks;
using StarCube.Game.Levels.Chunks;
using StarCube.Game.Levels.Chunks.Source;
using StarCube.Game.Levels.Generation;
using StarCube.Game.Levels.Loading;
using StarCube.Game.Levels.Storage;

namespace StarCube.Game.Levels
{
    public class ServerLevel : Level
    {
        public override bool HasBlock(int x, int y, int z)
        {
            return HasBlock(new BlockPos(x, y, z));
        }

        public override bool HasBlock(BlockPos pos)
        {
            return chunkSource.HasChunk(pos.GetChunkPos());
        }

        public override bool TryGetBlockState(int x, int y, int z, [NotNullWhen(true)] out BlockState? blockState)
        {
            return TryGetBlockState(new BlockPos(x, y, z), out blockState);
        }

        public override bool TryGetBlockState(BlockPos pos, [NotNullWhen(true)] out BlockState? blockState)
        {
            if (TryGetChunk(pos.GetChunkPos(), out Chunk? chunk))
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
            if (TryGetChunk(pos.GetChunkPos(), out Chunk? chunk))
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
            if (TryGetChunk(pos.GetChunkPos(), out Chunk? chunk))
            {
                oldBlockState = chunk.GetAndSetBlockState(pos.GetInChunkPos(), blockState);
                return true;
            }

            oldBlockState = null;
            return false;
        }


        public bool TryGetChunk(ChunkPos pos, [NotNullWhen(true)] out Chunk? chunk)
        {
            return chunkSource.TryGetChunk(pos, false, out chunk);
        }


        public void AddAnchor(ChunkLoadAnchor anchor)
        {
            chunkSource.AddAnchor(anchor);
        }

        public void RemoveAnchor(ChunkLoadAnchor anchor)
        {
            chunkSource.RemoveAnchor(anchor);
        }


        public override void Tick()
        {
            chunkSource.Tick();
        }

        public ServerLevel(Guid guid, ILevelBound bound, ILevelGenerator generator)
            : base(guid)
        {
            this.bound = bound;

            chunkSource = new ServerChunkSource(this, bound, generator, new LevelDataStorage());
        }

        public readonly ILevelBound bound;
        private readonly ServerChunkSource chunkSource;
    }
}
