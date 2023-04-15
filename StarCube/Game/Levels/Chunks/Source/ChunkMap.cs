using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using StarCube.Utility.Math;
using StarCube.Game.Levels.Chunks.Loading;

namespace StarCube.Game.Levels.Chunks.Source
{
    internal readonly struct ChunkMapEntry
    {
        /// <summary>
        /// 获取 chunk
        /// </summary>
        public Chunk Chunk => chunk ?? throw new NullReferenceException();

        /// <summary>
        /// Chunk 是否被加载进内存中
        /// </summary>
        public bool Loaded => chunk != null;

        /// <summary>
        /// Chunk 是否应该驻留在内存中
        /// </summary>
        public bool Alive => activeCount + loadCount > 0;

        /// <summary>
        /// Chunk 是否应该是活跃状态
        /// </summary>
        public bool Active => activeCount > 0;

        /// <summary>
        /// chunk 加载完毕
        /// </summary>
        /// <param name="chunk"></param>
        public ChunkMapEntry OnLoadChunk(Chunk chunk)
        {
            return new ChunkMapEntry(chunk, loadCount, activeCount);
        }

        /// <summary>
        /// 给 chunk 施加加载锚的影响，这会改变 chunk 的引用计数
        /// </summary>
        /// <param name="active"></param>
        public ChunkMapEntry OnAnchor(bool active)
        {
            if (active)
            {
                return new ChunkMapEntry(chunk, loadCount, activeCount + 1);
            }
            else
            {
                return new ChunkMapEntry(chunk, loadCount + 1, activeCount);
            }
        }

        /// <summary>
        /// 给 chunk 施加加载锚被移除的影响，这会改变 chunk 的引用计数
        /// </summary>
        /// <param name="active"></param>
        public ChunkMapEntry OnAnchorRemove(bool active)
        {
            if (active)
            {
                return new ChunkMapEntry(chunk, loadCount, activeCount - 1);
            }
            else
            {
                return new ChunkMapEntry(chunk, loadCount - 1, activeCount);
            }
        }


        public ChunkMapEntry(bool active)
        {
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

        private ChunkMapEntry(Chunk? chunk, int loadCount, int activeCount)
        {
            this.chunk = chunk;
            this.loadCount = loadCount;
            this.activeCount = activeCount;
        }

        private readonly int loadCount;
        private readonly int activeCount;

        private readonly Chunk? chunk;
    }

    public class ChunkMap
    {
        public bool IsLoaded(ChunkPos pos)
        {
            return posToChunkMapEntry.TryGetValue(pos, out ChunkMapEntry entry) && entry.Loaded;
        }

        public bool IsActive(ChunkPos pos)
        {
            return posToChunkMapEntry.TryGetValue(pos, out ChunkMapEntry entry) && entry.Active;
        }

        public bool IsLoading(ChunkPos pos)
        {
            return posToChunkMapEntry.TryGetValue(pos, out ChunkMapEntry entry) && !entry.Loaded;
        }

        /// <summary>
        /// 尝试获取一个已经被加载进内存的 chunk
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="chunk"></param>
        /// <returns></returns>
        public bool TryGet(ChunkPos pos, [NotNullWhen(true)] out Chunk? chunk)
        {
            if (posToChunkMapEntry.TryGetValue(pos, out ChunkMapEntry entry) && entry.Loaded)
            {
                chunk = entry.Chunk;
                return true;
            }

            chunk = null;
            return false;
        }

        /// <summary>
        /// 添加 anchor
        /// </summary>
        /// <param name="anchor"></param>
        public void AddAnchor(ChunkLoadAnchor anchor)
        {
            AnchorData data = anchor.Current;
            anchors.Add(new KeyValuePair<ChunkLoadAnchor, AnchorData>(anchor, data));
            OnAnchorAdd(data);
        }

        private void OnAnchorAdd(AnchorData data)
        {
            // 加载活跃区块
            for (int i = 0; i <= data.radius; ++i)
            {
                data.GetLoadChunkPos(i, chunkPosCache, bound);
                foreach (ChunkPos pos in chunkPosCache)
                {
                    OnChunkAnchored(pos, true);
                }
                chunkPosCache.Clear();
            }

            // 加载非活跃区块
            data.GetLoadChunkPos(data.radius + 1, chunkPosCache, bound);
            foreach (ChunkPos pos in chunkPosCache)
            {
                OnChunkAnchored(pos, false);
            }
            chunkPosCache.Clear();
        }

        private void OnChunkAnchored(ChunkPos pos, bool active)
        {
            // chunk 已被加载或在加载中，增加其加载计数
            if (posToChunkMapEntry.TryGetValue(pos, out ChunkMapEntry entry))
            {
                posToChunkMapEntry[pos] = entry.OnAnchor(active);
                return;
            }

            // chunk 没有加载，创建 ChunkMapEntry 并发出加载请求
            posToChunkMapEntry[pos] = new ChunkMapEntry(active);
            chunkProvider.Request(pos);
        }

        /// <summary>
        /// 移除 anchor
        /// </summary>
        /// <param name="anchor"></param>
        public void RemoveAnchor(ChunkLoadAnchor anchor)
        {
            for (int i = 0; i < anchors.Count; ++i)
            {
                if (anchors[i].Key == anchor)
                {
                    pendingRemoveAnchorData.Add(anchors[i].Value);
                    anchors.RemoveAt(i);
                    break;
                }
            }
        }

        private void OnAnchorRemove(AnchorData data)
        {
            // 卸载活跃区块
            for (int i = 0; i <= data.radius; ++i)
            {
                data.GetLoadChunkPos(i, chunkPosCache, bound);
                foreach (ChunkPos pos in chunkPosCache)
                {
                    OnChunkAnchorRemoved(pos, true);
                }
                chunkPosCache.Clear();
            }

            // 卸载非活跃区块
            data.GetLoadChunkPos(data.radius + 1, chunkPosCache, bound);
            foreach (ChunkPos pos in chunkPosCache)
            {
                OnChunkAnchorRemoved(pos, false);
            }
            chunkPosCache.Clear();
        }

        private void OnChunkAnchorRemoved(ChunkPos pos, bool active)
        {
            ChunkMapEntry entry = posToChunkMapEntry[pos];
            entry = entry.OnAnchorRemove(active);
            posToChunkMapEntry[pos] = entry;
            if (!entry.Alive)
            {
                pendingUnloadChunkPos.Add(pos);
            }
        }

        /// <summary>
        /// 更新 ChunkMap
        /// </summary>
        public void Update()
        {
            UpdateAnchors();

            UpdateChunkLoading();

            UpdateUnloadChunks();
        }

        /// <summary>
        /// 强制刷新所有加载中与等待加载的区块
        /// </summary>
        public void Flush()
        {
        }

        private void UpdateAnchors()
        {
            // 更新移动过的 anchor
            for (int i = 0; i < anchors.Count; ++i)
            {
                AnchorData origin = anchors[i].Value;
                AnchorData after = anchors[i].Key.Current;
                if (origin != after)
                {
                    OnAnchorMove(origin, after);
                    anchors[i] = new KeyValuePair<ChunkLoadAnchor, AnchorData>(anchors[i].Key, after);
                }
            }

            // 移除此 tick 期间移除的 anchor
            foreach (AnchorData anchorData in pendingRemoveAnchorData)
            {
                OnAnchorRemove(anchorData);
            }
            pendingRemoveAnchorData.Clear();
        }

        private void UpdateChunkLoading()
        {
            while (chunkProvider.TryGet(out Chunk? chunk))
            {
                // 此 chunk 已经不再需要加载，丢弃掉相应的结果
                if (!posToChunkMapEntry.TryGetValue(chunk.pos, out ChunkMapEntry entry))
                {
                    chunk.Clear();
                    continue;
                }

                // 更新 ChunkMapEntry
                posToChunkMapEntry[chunk.pos] = entry.OnLoadChunk(chunk);
                chunkHandler.OnChunkLoad(chunk, entry.Active);
            }

            //// 更新已完成的 chunk 任务
            //int loadingChunkCount = 0;
            //for (int i = 0; i < runningChunkTaskPos.Count; ++i)
            //{
            //    if (!posToChunkMapEntry.TryGetValue(runningChunkTaskPos[i], out ChunkMapEntry entry))
            //    {
            //        continue;
            //    }

            //    // 成功加载区块
            //    bool complete = entry.Update(false);
            //    if (complete)
            //    {
            //        chunkHandler.OnChunkLoad(entry.Chunk, entry.Active);
            //        continue;
            //    }

            //    // 仍在加载中
            //    runningChunkTaskPos[loadingChunkCount] = runningChunkTaskPos[i];
            //    loadingChunkCount++;
            //}
            //runningChunkTaskPos.RemoveRange(loadingChunkCount, runningChunkTaskPos.Count - loadingChunkCount);

            //// 创建并运行待加载的 chunk 任务
            //while (runningChunkTaskPos.Count < maxRunningChunkTask && pendingScheduleChunkTaskPos.TryDequeue(out ChunkPos pos))
            //{
            //    // 此 chunk 已经不再需要加载，跳过它
            //    if (!posToChunkMapEntry.TryGetValue(pos, out ChunkDataEntry? entry))
            //    {
            //        continue;
            //    }

            //    // 创建加载任务
            //    chunkProvider.Request(pos);
            //    //Task<Chunk> chunkTask = Task.Run(() => loadChunk(pos));
            //    //entry.OnScheduleChunkTask(chunkTask);
            //    runningChunkTaskPos.Add(pos);
            //}
        }

        private void UpdateUnloadChunks()
        {
            foreach (ChunkPos pos in pendingUnloadChunkPos)
            {
                // 如果由于锚点更新等原因导致 chunk 又被加载了，停止卸载此 chunk
                if (posToChunkMapEntry.TryGetValue(pos, out ChunkMapEntry entry) && entry.Alive)
                {
                    continue;
                }

                // 卸载区块
                if(posToChunkMapEntry.Remove(pos, out ChunkMapEntry removedEntry) && removedEntry.Loaded)
                {
                    ChunkPos removedPos = removedEntry.Chunk.pos;
                    removedEntry.Chunk.Clear();
                    chunkHandler.OnChunkUnload(removedPos);
                }
            }

            pendingUnloadChunkPos.Clear();
        }

        /// <summary>
        /// 当锚点移动时
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="after"></param>
        private void OnAnchorMove(AnchorData origin, AnchorData after)
        {
            // 简单实现，先放置新 anchor，再移除旧 anchor
            OnAnchorAdd(after);
            OnAnchorRemove(origin);
        }

        public void Stop()
        {
            chunkProvider.Stop();
        }

        public ChunkMap(ChunkProvider chunkProvider, ServerChunkSource chunkSource, ILevelBound bound, IChunkUpdateHandler chunkHandler)
        {
            this.chunkProvider = chunkProvider;
            chunkProvider.Start();
            this.chunkSource = chunkSource;
            this.chunkHandler = chunkHandler;

            this.bound = bound;

            chunkPosCache = new List<ChunkPos>(4096);
        }


        private readonly ChunkProvider chunkProvider;
        private readonly ServerChunkSource chunkSource;
        private readonly IChunkUpdateHandler chunkHandler;

        private readonly ILevelBound bound;

        private readonly Dictionary<ChunkPos, ChunkMapEntry> posToChunkMapEntry = new Dictionary<ChunkPos, ChunkMapEntry>();

        private readonly List<KeyValuePair<ChunkLoadAnchor, AnchorData>> anchors = new List<KeyValuePair<ChunkLoadAnchor, AnchorData>>();
        private readonly List<AnchorData> pendingRemoveAnchorData = new List<AnchorData>();

        private readonly HashSet<ChunkPos> pendingUnloadChunkPos = new HashSet<ChunkPos>();

        private readonly List<ChunkPos> chunkPosCache;
    }
}
