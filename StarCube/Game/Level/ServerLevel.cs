using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using StarCube.Utility.Math;
using StarCube.Game.Block;
using StarCube.Game.Level.Chunk;
using StarCube.Game.Level.Chunk.Source;
using StarCube.Game.Level.Generation;
using StarCube.Game.Level.Loading;

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
                    LevelChunk newChunk = loadingChunks[i].Result;
                    chunkMap.Add(newChunk.pos, newChunk);
                    continue;
                }

                loadingChunks[loadingChunkCount] = loadingChunks[i];
                loadingChunkCount++;
            }
            loadingChunks.RemoveRange(loadingChunkCount, loadingChunks.Count - loadingChunkCount);

            // 处理区块卸载
            foreach (LevelChunk chunk in chunkMap.Chunks)
            {
                bool alive = false;
                foreach (LevelLoadAnchor anchor in anchorToChunkPos.Keys)
                {
                    if (anchor.InRange(chunk.pos))
                    {
                        alive = true;
                        break;
                    }
                }

                if(!alive)
                {
                    chunkMap.Remove(chunk.pos);
                }
            }

            foreach (LevelLoadAnchor anchor in anchorToChunkPos.Keys)
            {
                ChunkPos chunkPos = anchor.GetChunkPos();
                int x0 = chunkPos.x - anchor.radius;
                int y0 = chunkPos.y - anchor.radius;
                int z0 = chunkPos.z - anchor.radius;
                int x1 = chunkPos.x + anchor.radius;
                int y1 = chunkPos.y + anchor.radius;
                int z1 = chunkPos.z + anchor.radius;
                for (int x = x0; x <= x1; x++)
                {
                    if(XOutOfBound(x))
                    {
                        continue;
                    }
                    for (int y = y0; y <= y1; y++)
                    {
                        if (YOutOfBound(y))
                        {
                            continue;
                        }
                        for (int z = z0; z <= z1; z++)
                        {
                            if (ZOutOfBound(z))
                            {
                                continue;
                            }
                            ChunkPos pos = new ChunkPos(x, y, z);
                            if(!chunkMap.Contains(pos))
                            {
                                chunksToLoad.Add(pos);
                            }
                        }
                    }
                }

                chunksToLoad.Sort((left, right) =>
                {
                    int l = Math.Abs(left.x - chunkPos.x) + Math.Abs(left.y - chunkPos.y) + Math.Abs(left.z - chunkPos.z);
                    int r = Math.Abs(right.x - chunkPos.x) + Math.Abs(right.y - chunkPos.y) + Math.Abs(right.z - chunkPos.z);
                    if (l == r)
                    {
                        return 0;
                    }
                    return l < r ? -1 : 1;
                });

                
            }
        }

        public void AddAnchor(LevelLoadAnchor anchor)
        {
            anchorToChunkPos.Add(anchor, anchor.GetChunkPos());
        }

        public void RemoveAnchor(LevelLoadAnchor anchor)
        {
            anchorToChunkPos.Remove(anchor);
        }

        private void LoadOrGenerateChunk(ChunkPos pos)
        {
            loadingChunks.Add(chunkSource.GetChunkAsync(pos));
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

            chunkSource = new ServerChunkSource(generator);

            chunkMap = new ChunkMap();

            loadingChunks = new List<Task<LevelChunk>>(256);

            anchorToChunkPos = new Dictionary<LevelLoadAnchor, ChunkPos>();
        }

        private readonly int widthInChunk;
        private readonly int yChunkMin;
        private readonly int heightInChunk;

        private readonly ServerChunkSource chunkSource;

        private readonly ChunkMap chunkMap;

        private readonly List<Task<LevelChunk>> loadingChunks;

        private readonly Dictionary<LevelLoadAnchor, ChunkPos> anchorToChunkPos;

        private readonly List<ChunkPos> chunksToLoad = new List<ChunkPos>();
    }
}
