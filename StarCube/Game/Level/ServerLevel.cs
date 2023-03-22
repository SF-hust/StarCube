using System;
using System.Diagnostics.CodeAnalysis;

using StarCube.Utility.Math;
using StarCube.Game.Block;
using StarCube.Game.Level.Chunk;
using StarCube.Game.Level.Chunk.Source;
using StarCube.Game.Level.Generation;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StarCube.Game.Level
{
    public class ServerLevel : WorldLevel
    {
        public override bool TryGetBlockState(int x, int y, int z, [NotNullWhen(true)] out BlockState? blockState)
        {
            return TryGetBlockState(new BlockPos(x, y, z), out blockState);
        }

        public override bool TryGetBlockState(BlockPos pos, [NotNullWhen(true)] out BlockState? blockState)
        {
            if (TryGetChunk(pos.ChunkPos, out LevelChunk? chunk))
            {
                blockState = chunk.GetBlockState(pos.InChunkPos);
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
            if (TryGetChunk(pos.ChunkPos, out LevelChunk? chunk))
            {
                chunk.SetBlockState(pos.InChunkPos, blockState);
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
            if (TryGetChunk(pos.ChunkPos, out LevelChunk? chunk))
            {
                oldBlockState = chunk.GetAndSetBlockState(pos.InChunkPos, blockState);
                return true;
            }

            oldBlockState = null;
            return false;
        }

        public override bool TryGetChunk(int x, int y, int z, [NotNullWhen(true)] out LevelChunk? chunk)
        {
            return TryGetChunk(new ChunkPos(x, y, z), out chunk);
        }

        public override bool TryGetChunk(ChunkPos pos, [NotNullWhen(true)] out LevelChunk? chunk)
        {
            if (chunkMap.TryGet(pos, out chunk))
            {
                return true;
            }

            chunk = null;
            return false;
        }

        public override void Tick()
        {
            base.Tick();

            // 处理加载完成的区块
            int loadingChunkCount = 0;
            for (int i = 0; i < loadingChunks.Count; ++i)
            {
                if (loadingChunks[i].IsCompleted)
                {
                    LevelChunk chunk = loadingChunks[i].Result;
                    chunkMap.Add(chunk.pos, chunk);
                    continue;
                }

                loadingChunks[loadingChunkCount] = loadingChunks[i];
                loadingChunkCount++;
            }
            loadingChunks.RemoveRange(loadingChunkCount, loadingChunks.Count - loadingChunkCount);

            // 处理区块加载

        }

        private void LoadOrGenerateChunk(ChunkPos pos)
        {
            loadingChunks.Add(chunkSource.GetChunkAsync(pos));
        }

        private bool ChunkOutOfBound(int x, int y, int z)
        {
            return y < yChunkMin || y >= yChunkMin + heightInChunk ||
                x < -widthInChunk || x >= widthInChunk ||
                z < -widthInChunk || z >= widthInChunk;
        }

        public ServerLevel(Guid guid, int widthInChunk, int yChunkMin, int heightInChunk, ILevelGenerator generator)
            : base(guid)
        {
            this.widthInChunk = widthInChunk;
            this.yChunkMin = yChunkMin;
            this.heightInChunk = heightInChunk;

            chunkSource = new ServerChunkSource(generator);

            chunkMap = new ChunkMap();

            loadingChunks = new List<Task<LevelChunk>>(256);
        }

        private readonly int widthInChunk;
        private readonly int yChunkMin;
        private readonly int heightInChunk;

        private readonly ServerChunkSource chunkSource;

        private readonly ChunkMap chunkMap;

        private readonly List<Task<LevelChunk>> loadingChunks;
    }
}
