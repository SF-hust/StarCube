using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using StarCube.Utility.Math;
using StarCube.Game.Levels.Chunks.Map;
using StarCube.Game.Levels.Chunks.Storage;
using StarCube.Game.Levels.Loading;
using StarCube.Game.Levels.Generation;

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
            anchors.Add(new KeyValuePair<ChunkLoadAnchor, AnchorData>(anchor, anchor.Current));
            chunkMap.AddAnchor(anchor);
        }

        public void RemoveAnchor(ChunkLoadAnchor anchor)
        {
            for (int i = 0; i < anchors.Count; i++)
            {
                if (anchors[i].Key == anchor)
                {
                    anchors.RemoveAt(i);
                    chunkMap.RemoveAnchor(anchor);
                    break;
                }
            }
        }

        public void ForceLoadByAnchor(ChunkLoadAnchor anchor)
        {
            bool found = false;
            for (int i = 0; i < anchors.Count; i++)
            {
                if (anchors[i].Key == anchor)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                return;
            }



            FlushLoading();
        }

        public override void Tick()
        {
            UpdateAnchors();
        }

        private void UpdateAnchors()
        {
            for (int i = 0; i < anchors.Count; ++i)
            {
                var pair = anchors[i];
                ChunkLoadAnchor anchor = pair.Key;
                if (anchor.Current != pair.Value)
                {
                    chunkMap.AddAnchor(anchor);
                    chunkMap.RemoveAnchor(anchor);


                    anchors[i] = new KeyValuePair<ChunkLoadAnchor, AnchorData>(anchor, anchor.Current);
                }
            }
        }

        public void FlushLoading()
        {
            chunkMap.FlushLoading();
        }

        private Task<Chunk> LoadOrGenerateChunk(ChunkPos pos)
        {
            return new Task<Chunk>(() => generator.GenerateChunk(pos));
        }

        public ServerChunkSource(ServerLevel level, ILevelGenerator generator, ChunkStorage storage)
        {
            this.level = level;

            this.generator = generator;
            this.storage = storage;

            chunkMap = new ChunkMap((pos) => LoadOrGenerateChunk(pos));
            anchors = new List<KeyValuePair<ChunkLoadAnchor, AnchorData>>();
        }

        public readonly ServerLevel level;

        private readonly ILevelGenerator generator;
        private readonly ChunkStorage storage;

        private readonly ChunkMap chunkMap;

        private readonly List<KeyValuePair<ChunkLoadAnchor, AnchorData>> anchors;
    }
}
