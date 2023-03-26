﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using StarCube.Utility.Math;
using StarCube.Game.Level.Chunk.Map;
using StarCube.Game.Level.Chunk.Storage;
using StarCube.Game.Level.Loading;
using StarCube.Game.Level.Generation;

namespace StarCube.Game.Level.Chunk.Source
{
    public sealed class ServerChunkSource : ChunkSource
    {
        public override bool HasChunk(ChunkPos pos)
        {
            return chunkMap.IsLoaded(pos);
        }

        public override bool TryGetChunk(ChunkPos pos, bool load, [NotNullWhen(true)] out LevelChunk? chunk)
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

        private Task<LevelChunk> LoadOrGenerateChunk(ChunkPos pos)
        {
            return new Task<LevelChunk>(() => generator.GenerateChunk(pos));
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
