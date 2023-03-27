using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using StarCube.Utility.Math;
using StarCube.Game.Level.Loading;

namespace StarCube.Game.Level.Chunks.Map
{
    internal class ChunkDataEntry
    {
        public bool Loaded => chunk != null;
        public bool Alive => Active || loadCount > 0;
        public bool Active => activeCount > 0;

        public bool Update(bool sync = false)
        {
            if (loadingChunk != null && (sync || loadingChunk.IsCompleted))
            {
                chunk = loadingChunk.Result;
                loadingChunk = null;
                return true;
            }

            return false;
        }

        public void OnAnchor(bool active)
        {
            if (active)
            {
                activeCount++;
            }
            else
            {
                loadCount++;
            }
        }

        public void OnAnchorRemove(bool active)
        {
            if (active)
            {
                activeCount--;
            }
            else
            {
                loadCount--;
            }
        }

        public ChunkDataEntry(Chunk chunk, bool active)
        {
            this.chunk = chunk;
            loadingChunk = null;
            if (active)
            {
                loadCount = 0;
                activeCount = 1;
            }
            else
            {
                loadCount = 1;
                activeCount = 0;
            }
        }

        public ChunkDataEntry(Task<Chunk> loadingChunk, bool active)
        {
            this.loadingChunk = loadingChunk;
            chunk = null;
            if (active)
            {
                loadCount = 0;
                activeCount = 1;
            }
            else
            {
                loadCount = 1;
                activeCount = 0;
            }
        }

        public Chunk Chunk => chunk ?? throw new NullReferenceException();

        private Chunk? chunk;
        private Task<Chunk>? loadingChunk;

        private int loadCount;
        private int activeCount;
    }

    internal struct UpdatingAnchorData
    {
        public int Radius => radius;

        public int Current
        {
            get => current;
            set => current = (ushort)value;
        }

        public void Update()
        {
            current++;
        }

        public void GetChunkPosForCurrentUpdate(List<ChunkPos> positions)
        {
            if(Current == 0)
            {
                positions.Add(chunkPos);
                return;
            }

            // 前后
            for (int x = (chunkPos.x - current); x <= (chunkPos.x + current); ++x)
            {
                for (int y = (chunkPos.y - current); y <= (chunkPos.y + current); ++y)

                {
                    positions.Add(new ChunkPos(x, y, chunkPos.z + current));
                    positions.Add(new ChunkPos(x, y, chunkPos.z - current));
                }
            }
            // 左右
            for (int z = (chunkPos.z - current + 1); z <= (chunkPos.z + current - 1); ++z)
            {
                for (int y = (chunkPos.y - current); y <= (chunkPos.y + current); ++y)
                {
                    positions.Add(new ChunkPos(chunkPos.x + current, y, z));
                    positions.Add(new ChunkPos(chunkPos.x - current, y, z));
                }
            }
            // 上下
            for (int x = (chunkPos.x - current + 1); x <= (chunkPos.x + current - 1); ++x)
            {
                for (int z = (chunkPos.z - current + 1); z <= (chunkPos.z + current - 1); ++z)
                {
                    positions.Add(new ChunkPos(x, chunkPos.y + current, z));
                    positions.Add(new ChunkPos(x, chunkPos.y - current, z));
                }
            }
        }

        public UpdatingAnchorData(ChunkPos pos, int radius)
        {
            anchor = null;
            chunkPos = pos;
            this.radius = (ushort)radius;
            current = 0;
        }

        public UpdatingAnchorData(ChunkLoadAnchor anchor)
        {
            this.anchor = anchor;
            chunkPos = anchor.ChunkPos;
            radius = (ushort)anchor.Radius;
            current = 0;
        }

        public readonly ChunkLoadAnchor? anchor;

        public readonly ChunkPos chunkPos;

        private readonly ushort radius;
        private ushort current;
    }


    public class ChunkMap
    {
        public bool IsLoaded(ChunkPos pos)
        {
            return posToChunkDataEntry.TryGetValue(pos, out ChunkDataEntry chunkDataEntry) && chunkDataEntry.Loaded;
        }

        public bool IsActive(ChunkPos pos)
        {
            return posToChunkDataEntry.TryGetValue(pos, out ChunkDataEntry chunkDataEntry) && chunkDataEntry.Active;
        }

        public bool IsLoading(ChunkPos pos)
        {
            return posToChunkDataEntry.TryGetValue(pos, out ChunkDataEntry chunkDataEntry) && !chunkDataEntry.Loaded;
        }

        public bool TryGet(ChunkPos pos, [NotNullWhen(true)] out Chunk? chunk)
        {
            if(posToChunkDataEntry.TryGetValue(pos, out ChunkDataEntry? entry) && entry.Loaded)
            {
                chunk = entry.Chunk;
                return true;
            }

            chunk = null;
            return false;
        }

        public void AddAnchor(ChunkLoadAnchor anchor)
        {
            updatingAnchors.Add(new UpdatingAnchorData(anchor));
        }

        public void RemoveAnchor(ChunkLoadAnchor anchor)
        {
            // 此 anchor 正在更新中
            for (int i = 0; i < updatingAnchors.Count; ++i)
            {
                if (updatingAnchors[i].anchor == anchor)
                {
                    UpdatingAnchorData data = updatingAnchors[i];
                    OnUpdatingAnchorRemove(ref data);
                    updatingAnchors.RemoveAt(i);
                    return;
                }
            }

            // 此 anchor 已经更新完毕
            UpdatingAnchorData anchorUpdateData = new UpdatingAnchorData(anchor.ChunkPos, anchor.Radius)
            {
                Current = anchor.Radius
            };
            OnUpdatingAnchorRemove(ref anchorUpdateData);
        }


        public void Update()
        {
            UpdateLoadingChunks();

            // 正在加载的区块数量小于 256 时才加载更多区块，且尽可能让正在加载的区块数量不小于 64
            if(loadingChunkPos.Count < 256)
            {
                while (updatingAnchors.Count > 0 && loadingChunkPos.Count < 64)
                {
                    UpdateAnchors();
                }
            }
        }

        public void FlushLoading()
        {
            foreach (ChunkPos pos in loadingChunkPos)
            {
                posToChunkDataEntry[pos].Update(true);
            }

            loadingChunkPos.Clear();
        }


        private void OnUpdatingAnchorRemove(ref UpdatingAnchorData updatingAnchorData)
        {
            while (updatingAnchorData.Current >= 0)
            {
                updatingAnchorData.GetChunkPosForCurrentUpdate(toUpdateChunkPos);
                OnChunksReceiveAnchorRemove(toUpdateChunkPos, updatingAnchorData.Current != updatingAnchorData.Radius);
                updatingAnchorData.Current--;
            }
        }

        private void OnChunksReceiveAnchorRemove(List<ChunkPos> positions, bool active)
        {
            foreach (ChunkPos pos in positions)
            {
                ChunkDataEntry entry = posToChunkDataEntry[pos];
                entry.OnAnchorRemove(active);
                if (!entry.Alive)
                {
                    posToChunkDataEntry.Remove(pos);
                }
            }
        }


        private void UpdateLoadingChunks()
        {
            int loadingChunkCount = 0;
            for (int i = 0; i < loadingChunkPos.Count; ++i)
            {
                bool complete = posToChunkDataEntry[loadingChunkPos[i]].Update(false);
                if (complete)
                {
                    continue;
                }

                loadingChunkPos[loadingChunkCount] = loadingChunkPos[i];
                loadingChunkCount++;
            }

            loadingChunkPos.RemoveRange(loadingChunkPos.Count - loadingChunkCount, loadingChunkCount);
        }

        private void UpdateAnchors()
        {
            int updatingAnchorCount = 0;
            for (int i = 0; i < updatingAnchors.Count; ++i)
            {
                bool complete = UpdateAnchor(i);
                if (complete)
                {
                    continue;
                }

                updatingAnchors[updatingAnchorCount] = updatingAnchors[i];
                updatingAnchorCount++;
            }

            loadingChunkPos.RemoveRange(loadingChunkPos.Count - updatingAnchorCount, updatingAnchorCount);
        }

        private bool UpdateAnchor(int index)
        {
            UpdatingAnchorData anchorUpdateData = updatingAnchors[index];
            // 一次只更新一层
            anchorUpdateData.GetChunkPosForCurrentUpdate(toUpdateChunkPos);
            foreach (ChunkPos pos in toUpdateChunkPos)
            {
                // 相应区块已被加载或正在加载中，增加相应的加载计数
                if (posToChunkDataEntry.TryGetValue(pos, out ChunkDataEntry? entry))
                {
                    entry.OnAnchor(anchorUpdateData.Current != anchorUpdateData.Radius);
                }
                // 相应的区块既没有被加载，也不在加载中，创建一个 entry
                else
                {
                    entry = new ChunkDataEntry(createChunkTask(pos), anchorUpdateData.Current != anchorUpdateData.Radius);
                    posToChunkDataEntry.Add(pos, entry);
                    loadingChunkPos.Add(pos);
                }
            }
            anchorUpdateData.Current++;
            updatingAnchors[index] = anchorUpdateData;

            return anchorUpdateData.Current == anchorUpdateData.Radius;
        }


        public ChunkMap(Func<ChunkPos, Task<Chunk>> chunkTaskCreator)
        {
            createChunkTask = chunkTaskCreator;
            posToChunkDataEntry = new Dictionary<ChunkPos, ChunkDataEntry>();
            loadingChunkPos = new List<ChunkPos>();

            anchors = new List<KeyValuePair<ChunkLoadAnchor, AnchorData>>();
            updatingAnchors = new List<UpdatingAnchorData>();
            toUpdateChunkPos = new List<ChunkPos>();
        }


        private readonly Func<ChunkPos, Task<Chunk>> createChunkTask;

        private readonly Dictionary<ChunkPos, ChunkDataEntry> posToChunkDataEntry;

        private readonly List<ChunkPos> loadingChunkPos;

        private readonly List<KeyValuePair<ChunkLoadAnchor, AnchorData>> anchors;

        private readonly List<UpdatingAnchorData> updatingAnchors;
        private readonly List<ChunkPos> toUpdateChunkPos;
    }
}
