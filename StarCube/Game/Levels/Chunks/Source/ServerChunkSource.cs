using System.Diagnostics.CodeAnalysis;

using StarCube.Utility.Math;
using StarCube.Game.Levels.Generation;
using StarCube.Game.Levels.Storage;
using StarCube.Game.Levels.Chunks.Loading;

namespace StarCube.Game.Levels.Chunks.Source
{
    public sealed class ServerChunkSource : ChunkSource
    {
        public override bool HasChunk(ChunkPos pos)
        {
            return chunkMap.IsLoaded(pos);
        }

        public override bool TryGetChunk(ChunkPos pos, [NotNullWhen(true)] out Chunk? chunk)
        {
            return chunkMap.TryGet(pos, out chunk);
        }

        public bool TryAddAnchor(ChunkLoadAnchor anchor)
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

        public void Stop()
        {
            chunkMap.Stop();
        }

        public ServerChunkSource(ChunkedServerLevel level, ILevelChunkGenerator generator, LevelStorage storage)
        {
            this.level = level;

            chunkMap = new ChunkMap(new ChunkProvider(generator, storage, 4096), level.bounding);
        }

        public readonly ChunkedServerLevel level;

        private readonly ChunkMap chunkMap;
    }
}
