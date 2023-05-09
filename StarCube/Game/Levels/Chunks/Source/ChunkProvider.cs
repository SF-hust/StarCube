using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

using StarCube.Utility.Math;
using StarCube.Utility.Logging;
using StarCube.Game.Levels.Generation;
using StarCube.Game.Levels.Storage;

namespace StarCube.Game.Levels.Chunks.Source
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ChunkProvider : IDisposable
    {
        public void Start()
        {
            workerTask.Start();
        }

        public void Request(ChunkPos pos)
        {
            pendingLoadChunkPos.Enqueue(pos);
        }

        public bool TryGet( [NotNullWhen(true)] out Chunk? chunk)
        {
            return chunkResults.TryDequeue(out chunk);
        }

        public void Gets(List<Chunk> chunks)
        {
            while (chunkResults.TryDequeue(out Chunk? chunk))
            {
                chunks.Add(chunk);
            }
        }

        private Chunk GetSync(ChunkPos pos)
        {
            if (storage != null && storage.TryLoadChunk(pos, out Chunk? chunk))
            {
                return chunk;
            }

            chunk = generator.GenerateChunk(pos);
            chunk.Modify = true;
            return chunk;
        }

        public void Stop()
        {
            stop = true;
            workerTask.Wait();
        }

        private void Work(int _)
        {
            if (pendingLoadChunkPos.TryDequeue(out ChunkPos pos))
            {
                Chunk chunk = GetSync(pos);
                chunkResults.Enqueue(chunk);
            }
        }

        private void WorkerRun()
        {
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                //Thread.CurrentThread.Priority = ThreadPriority.Lowest;
                while (!stop)
                {
                    stopwatch.Restart();
                    //if (pendingLoadChunkPos.Count > 0 && chunkResults.Count < maxResultCount)
                    //{
                    //    Parallel.For(0, maxResultCount - chunkResults.Count, Work);
                    //    continue;
                    //}
                    while (pendingLoadChunkPos.TryDequeue(out ChunkPos pos))
                    {
                        Chunk chunk = GetSync(pos);
                        chunkResults.Enqueue(chunk);
                    }
                    stopwatch.Stop();
                    int ms = (int)stopwatch.ElapsedMilliseconds;
                    if (ms < taskSleepMilliseconds)
                    {
                        Thread.Sleep(taskSleepMilliseconds - ms);
                    }
                }
            }
            catch (Exception e)
            {
                LogUtil.Error(e);
                throw e;
            }
        }

        public void Dispose()
        {
            stop = true;
            workerTask.Wait();
        }

        public ChunkProvider(ILevelChunkGenerator generator, LevelStorage storage, int maxResultCount)
        {
            this.generator = generator;
            this.storage = storage;
            this.maxResultCount = maxResultCount;
            workerTask = new Task(WorkerRun, TaskCreationOptions.LongRunning);
            taskSleepMilliseconds = 5;
        }

        private readonly ILevelChunkGenerator generator;

        private readonly LevelStorage storage;

        private readonly ConcurrentQueue<ChunkPos> pendingLoadChunkPos = new ConcurrentQueue<ChunkPos>();

        private readonly ConcurrentQueue<Chunk> chunkResults = new ConcurrentQueue<Chunk>();

        private readonly int maxResultCount;

        private readonly int taskSleepMilliseconds;

        private readonly Task workerTask;

        private volatile bool stop = false;
    }
}
