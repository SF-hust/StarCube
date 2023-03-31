using System.Diagnostics.CodeAnalysis;

using StarCube.Utility.Math;
using StarCube.Game.Levels.Loading;
using StarCube.Game.Levels.Generation;
using StarCube.Game.Levels.Storage;

namespace StarCube.Game.Levels.Chunks.Source
{
    public sealed class ServerChunkSource : ChunkSource
    {
        public override bool HasChunk(ChunkPos pos)
        {
            return chunkMap.IsLoaded(pos);
        }

        public override bool TryGetChunk(ChunkPos pos, bool load, [NotNullWhen(true)] out Chunk? chunk)
        {
            return chunkMap.TryGet(pos, out chunk);
        }

        public void AddAnchor(ChunkLoadAnchor anchor)
        {
            chunkMap.AddAnchor(anchor);
        }

        public void RemoveAnchor(ChunkLoadAnchor anchor)
        {
            chunkMap.RemoveAnchor(anchor);
        }

        public override void Tick()
        {
            chunkMap.Update();
        }

        public void FlushChunks()
        {
            chunkMap.Flush();
        }

        private Chunk LoadOrGenerateChunk(ChunkPos pos)
        {
            if(storage.TryLoadChunk(pos, out Chunk? chunk))
            {
                return chunk;
            }

            chunk = generator.GenerateChunk(pos);
            return chunk;
        }

        public ServerChunkSource(ServerLevel level, ILevelBound bound, ILevelGenerator generator, LevelDataStorage storage)
        {
            this.level = level;

            this.bound = bound;

            this.generator = generator;
            this.storage = storage;

            chunkMap = new ChunkMap(LoadOrGenerateChunk, bound);
        }

        public readonly ServerLevel level;

        private readonly ILevelBound bound;

        private readonly ILevelGenerator generator;
        private readonly LevelDataStorage storage;

        private readonly ChunkMap chunkMap;
    }
}
