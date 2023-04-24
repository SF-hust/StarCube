using System;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;

using StarCube.Utility.Math;
using StarCube.Game.Blocks;
using StarCube.Game.Levels.Generation;
using StarCube.Game.Levels.Storage;
using StarCube.Game.Levels.Chunks;
using StarCube.Game.Levels.Chunks.Source;
using StarCube.Game.Levels.Chunks.Loading;
using StarCube.Game.Worlds;

namespace StarCube.Game.Levels
{
    public sealed class ChunkedServerLevel : ServerLevel, IChunkedServerLevel
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
            if (chunkSource.TryGetChunk(pos.GetChunkPos(), out Chunk? chunk))
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
            if (chunkSource.TryGetChunk(pos.GetChunkPos(), out Chunk? chunk))
            {
                chunk.SetBlockState(pos.GetInChunkPos(), blockState);
                chunkSource.OnChunkModify(chunk);
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
            if (chunkSource.TryGetChunk(pos.GetChunkPos(), out Chunk? chunk))
            {
                oldBlockState = chunk.GetAndSetBlockState(pos.GetInChunkPos(), blockState);
                chunkSource.OnChunkModify(chunk);
                return true;
            }

            oldBlockState = null;
            return false;
        }

        public override bool HasChunk(ChunkPos pos)
        {
            return chunkSource.HasChunk(pos);
        }

        public override bool TryGetChunk(ChunkPos pos, [NotNullWhen(true)] out Chunk? chunk)
        {
            return chunkSource.TryGetChunk(pos, out chunk);
        }

        public bool TryAddDynamicAnchor(ChunkLoadAnchor anchor)
        {
            return chunkSource.TryAddAnchor(anchor);
        }

        public bool TryRemoveDynamicAnchor(ChunkLoadAnchor anchor)
        {
            chunkSource.RemoveAnchor(anchor);
            return true;
        }

        public long AddStaticAnchor(AnchorData anchorData)
        {
            long id = currentStaticAnchorID;
            currentStaticAnchorID++;
            idToStaticAnchor.Add(id, anchorData);
            return id;
        }

        public bool TryRemoveStaticAnchor(long id)
        {
            return idToStaticAnchor.Remove(id);
        }

        public int RemoveStaticAnchorsAt(ChunkPos pos)
        {
            foreach (var pair in idToStaticAnchor)
            {
                if (pos == pair.Value.chunkPos)
                {
                    toRemoveStaticAnchorIDCache.Add(pair.Key);
                }
            }
            int count = toRemoveStaticAnchorIDCache.Count;
            foreach (long id in toRemoveStaticAnchorIDCache)
            {
                idToStaticAnchor.Remove(id);
            }
            return count;
        }

        public override void Tick()
        {
            chunkSource.Tick();
        }

        public void Stop()
        {
            chunkSource.Stop();
        }

        protected override void DoSave()
        {
            chunkSource.Save();
        }

        public ChunkedServerLevel(Guid guid, ILevelBounding bounding, ServerWorld world, ILevelChunkGenerator generator, LevelStorage storage)
            : base(guid, bounding, world, generator, storage)
        {
            chunkSource = new ServerChunkCache(this, generator, storage);
        }

        private readonly Dictionary<long, AnchorData> idToStaticAnchor = new Dictionary<long, AnchorData>();

        private long currentStaticAnchorID = 1;

        private readonly List<long> toRemoveStaticAnchorIDCache = new List<long>();

        private readonly ServerChunkCache chunkSource;
    }
}
