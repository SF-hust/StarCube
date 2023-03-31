using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using StarCube.Utility.Math;
using StarCube.Game.Levels.Loading;

namespace StarCube.Game.Levels.Chunks.Source
{
    internal class ChunkDataEntry
    {
        /// <summary>
        /// chunk 的加载任务是否已经开始
        /// </summary>
        public bool Scheduled => chunkTask != null;

        /// <summary>
        /// Chunk 是否已经在内存中
        /// </summary>
        public bool Loaded => chunk != null;

        /// <summary>
        /// Chunk 是否应该留在内存中
        /// </summary>
        public bool Alive => activeCount + loadCount > 0;

        /// <summary>
        /// Chunk 是否应该是活跃状态
        /// </summary>
        public bool Active => activeCount > 0;

        /// <summary>
        /// 如果此 chunk 正在加载中，尝试获取加载结果
        /// </summary>
        /// <param name="flush">是否要以阻塞的方式强制刷新</param>
        /// <returns></returns>
        public bool Update(bool flush = false)
        {
            if (chunkTask == null)
            {
                return false;
            }

            if (!chunkTask.IsCompleted)
            {
                if (flush)
                {
                    chunkTask.Wait();
                }
                else
                {
                    return false;
                }
            }

            chunk = chunkTask.Result;
            chunkTask = null;
            return true;
        }

        /// <summary>
        /// 为 entry 设置 chunk 加载任务
        /// </summary>
        /// <param name="chunkTask"></param>
        public void OnScheduleChunkTask(Task<Chunk> chunkTask)
        {
            this.chunkTask = chunkTask;
        }

        public void OnLoadChunk(Chunk chunk)
        {
            this.chunk = chunk;
        }

        /// <summary>
        /// 给 chunk 施加加载锚的影响，这会改变 chunk 的引用计数
        /// </summary>
        /// <param name="active"></param>
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

        /// <summary>
        /// 给 chunk 施加加载锚被移除的影响，这会改变 chunk 的引用计数
        /// </summary>
        /// <param name="active"></param>
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

        /// <summary>
        /// 创建一个包含有已加载 chunk 数据的 entry
        /// </summary>
        /// <param name="chunk"></param>
        /// <param name="active"></param>
        public ChunkDataEntry(Chunk chunk, bool active) : this(active)
        {
            this.chunk = chunk;
        }

        /// <summary>
        /// 创建一个正在加载的 chunk 的 entry
        /// </summary>
        /// <param name="chunkTask"></param>
        /// <param name="active"></param>
        public ChunkDataEntry(Task<Chunk> chunkTask, bool active) : this(active)
        {
            this.chunkTask = chunkTask;
        }

        /// <summary>
        /// 创建一个等待被 schedule 的加载中 chunk 的 entry
        /// </summary>
        /// <param name="active"></param>
        public ChunkDataEntry(bool active)
        {
            chunk = null;
            chunkTask = null;

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

        /// <summary>
        /// 获取已加载好的 chunk
        /// </summary>
        public Chunk Chunk => chunk ?? throw new NullReferenceException();

        private Chunk? chunk;
        private Task<Chunk>? chunkTask;

        private int loadCount;
        private int activeCount;
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

        /// <summary>
        /// 尝试获取一个已经被加载进内存的 chunk
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="chunk"></param>
        /// <returns></returns>
        public bool TryGet(ChunkPos pos, [NotNullWhen(true)] out Chunk? chunk)
        {
            if (posToChunkDataEntry.TryGetValue(pos, out ChunkDataEntry? entry) && entry.Loaded)
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
                data.GetLoadChunkPos(i, chunkPosCache);
                foreach (ChunkPos pos in chunkPosCache)
                {
                    OnChunkAnchored(pos, true);
                }
                chunkPosCache.Clear();
            }

            // 加载非活跃区块
            data.GetLoadChunkPos(data.radius + 1, chunkPosCache);
            foreach (ChunkPos pos in chunkPosCache)
            {
                OnChunkAnchored(pos, false);
            }
            chunkPosCache.Clear();
        }

        private void OnChunkAnchored(ChunkPos pos, bool active)
        {
            if(!bound.InRange(pos))
            {
                return;
            }

            // chunk 已经在加载中，增加其加载计数
            if (posToChunkDataEntry.TryGetValue(pos, out ChunkDataEntry? entry))
            {
                entry.OnAnchor(active);
                return;
            }

            // 运行中的 chunk 加载任务数量少于上限，创建任务并运行
            if (runningChunkTaskPos.Count < maxRunningChunkTask)
            {
                Task<Chunk> chunkTask = new Task<Chunk>(() => loadChunk(pos));
                chunkTask.Start();
                posToChunkDataEntry[pos] = new ChunkDataEntry(chunkTask, active);
                runningChunkTaskPos.Add(pos);
                return;
            }

            // 运行中的 chunk 加载任务数量已达到上限，先放进队列中
            posToChunkDataEntry[pos] = new ChunkDataEntry(active);
            pendingScheduleChunkTaskPos.Enqueue(pos);
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
                data.GetLoadChunkPos(i, chunkPosCache);
                foreach (ChunkPos pos in chunkPosCache)
                {
                    OnChunkAnchorRemoved(pos, true);
                }
                chunkPosCache.Clear();
            }

            // 卸载非活跃区块
            data.GetLoadChunkPos(data.radius + 1, chunkPosCache);
            foreach (ChunkPos pos in chunkPosCache)
            {
                OnChunkAnchorRemoved(pos, false);
            }
            chunkPosCache.Clear();
        }

        private void OnChunkAnchorRemoved(ChunkPos pos, bool active)
        {
            if(!bound.InRange(pos))
            {
                return;
            }

            ChunkDataEntry entry = posToChunkDataEntry[pos];
            entry.OnAnchorRemove(active);
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
            // 刷新所有运行中的 chunk 加载任务
            foreach (ChunkPos pos in runningChunkTaskPos)
            {
                posToChunkDataEntry[pos].Update(true);
            }
            runningChunkTaskPos.Clear();

            // 运行并刷新所有等待运行的任务
            foreach (ChunkPos pos in pendingScheduleChunkTaskPos)
            {
                Chunk chunk = loadChunk(pos);
                posToChunkDataEntry[pos].OnLoadChunk(chunk);
            }
            pendingScheduleChunkTaskPos.Clear();
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
            // 更新已完成的 chunk 任务
            int loadingChunkCount = 0;
            for (int i = 0; i < runningChunkTaskPos.Count; ++i)
            {
                // 此 chunk 已经不再需要加载，丢弃掉相应的结果
                if (!posToChunkDataEntry.TryGetValue(runningChunkTaskPos[i], out ChunkDataEntry? entry))
                {
                    continue;
                }

                bool complete = entry.Update(false);
                if (complete)
                {
                    continue;
                }

                runningChunkTaskPos[loadingChunkCount] = runningChunkTaskPos[i];
                loadingChunkCount++;
            }
            runningChunkTaskPos.RemoveRange(runningChunkTaskPos.Count - loadingChunkCount, loadingChunkCount);

            // 创建并运行待加载的 chunk 任务
            while (runningChunkTaskPos.Count < maxRunningChunkTask && pendingScheduleChunkTaskPos.TryDequeue(out ChunkPos pos))
            {
                // 此 chunk 已经不再需要加载，跳过它
                if (!posToChunkDataEntry.TryGetValue(pos, out ChunkDataEntry? entry))
                {
                    continue;
                }

                Task<Chunk> chunkTask = new Task<Chunk>(() => loadChunk(pos));
                entry.OnScheduleChunkTask(chunkTask);
            }
        }

        private void UpdateUnloadChunks()
        {
            foreach (ChunkPos pos in pendingUnloadChunkPos)
            {
                // 如果由于锚点更新等原因导致 chunk 又被加载了，停止卸载此 chunk
                if (posToChunkDataEntry.TryGetValue(pos, out ChunkDataEntry? entry) && entry.Alive)
                {
                    continue;
                }

                posToChunkDataEntry.Remove(pos);
            }

            pendingUnloadChunkPos.Clear();
        }

        private void OnAnchorMove(AnchorData origin, AnchorData after)
        {
            // 简单实现，先放置新 anchor，再移除旧 anchor
            OnAnchorAdd(after);
            OnAnchorRemove(origin);
        }



        public ChunkMap(Func<ChunkPos, Chunk> chunkLoader, ILevelBound bound)
        {
            loadChunk = chunkLoader;

            this.bound = bound;

            posToChunkDataEntry = new Dictionary<ChunkPos, ChunkDataEntry>();

            anchors = new List<KeyValuePair<ChunkLoadAnchor, AnchorData>>();
            pendingRemoveAnchorData = new List<AnchorData>();

            runningChunkTaskPos = new List<ChunkPos>(maxRunningChunkTask);
            pendingScheduleChunkTaskPos = new Queue<ChunkPos>(8192);

            pendingUnloadChunkPos = new HashSet<ChunkPos>();

            chunkPosCache = new List<ChunkPos>(4096);
        }


        private readonly Func<ChunkPos, Chunk> loadChunk;

        private readonly ILevelBound bound;

        private readonly Dictionary<ChunkPos, ChunkDataEntry> posToChunkDataEntry;

        private readonly List<KeyValuePair<ChunkLoadAnchor, AnchorData>> anchors;
        private readonly List<AnchorData> pendingRemoveAnchorData;

        private readonly int maxRunningChunkTask = 256;
        private readonly List<ChunkPos> runningChunkTaskPos;
        private readonly Queue<ChunkPos> pendingScheduleChunkTaskPos;

        private readonly HashSet<ChunkPos> pendingUnloadChunkPos;

        private readonly List<ChunkPos> chunkPosCache;
    }
}
