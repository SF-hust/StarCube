using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using StarCube.Utility.Math;
using StarCube.Game.Block;
using StarCube.Game.Level.Chunks;
using StarCube.Game.Level.Chunks.Source;
using StarCube.Game.Level.Chunks.Storage;
using StarCube.Game.Level.Generation;
using StarCube.Game.Level.Loading;

namespace StarCube.Game.Level
{
    public class ServerLevel : WorldLevel
    {
        public override bool HasBlock(int x, int y, int z)
        {
            return HasBlock(new BlockPos(x, y, z));
        }

        public override bool HasBlock(BlockPos pos)
        {
            throw new NotImplementedException();
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


        private bool TryGetChunk(ChunkPos pos, [NotNullWhen(true)] out Chunk? chunk)
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

        private bool ChunkOutOfBound(int x, int y, int z)
        {
            return YOutOfBound(y) || XOutOfBound(x) || ZOutOfBound(z);
        }

        private bool XOutOfBound(int x)
        {
            return x < -widthInChunk || x >= widthInChunk;
        }

        private bool YOutOfBound(int y)
        {
            return y < yChunkMin || y >= yChunkMin + heightInChunk;
        }

        private bool ZOutOfBound(int z)
        {
            return z < -widthInChunk || z >= widthInChunk;
        }

        public ServerLevel(Guid guid, int widthInChunk, int yChunkMin, int heightInChunk, ILevelGenerator generator)
            : base(guid)
        {
            this.widthInChunk = widthInChunk;
            this.yChunkMin = yChunkMin;
            this.heightInChunk = heightInChunk;

            chunkSource = new ServerChunkSource(this, generator, new ChunkStorage());
        }

        private readonly int widthInChunk;
        private readonly int yChunkMin;
        private readonly int heightInChunk;

        private readonly ServerChunkSource chunkSource;
    }
}
