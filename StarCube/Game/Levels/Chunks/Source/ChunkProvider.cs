using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
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
    public class ChunkProvider
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

        public Chunk GetSync(ChunkPos pos)
        {
            if (storage.TryLoadChunk(pos, out Chunk? chunk))
            {
                return chunk;
            }

            return generator.GenerateChunk(pos);
        }

        public void Stop()
        {
            stop = true;
            workerTask.Wait();
        }

        private void Work()
        {
            if (pendingLoadChunkPos.TryDequeue(out ChunkPos pos))
            {
                Chunk chunk = GetSync(pos);
                chunkResults.Enqueue(chunk);
            }
        }

        private void WorkerRun()
        {
            int count64 = 0;
            int count16 = 0;
            int count4 = 0;
            int count = 0;
            try
            {
                Action action = Work;
                Action[] actions64 = new Action[64];
                Action[] actions16 = new Action[16];
                Action[] actions4 = new Action[16];
                actions64.AsSpan().Fill(action);
                actions16.AsSpan().Fill(action);
                actions4.AsSpan().Fill(action);

                while (!stop)
                {
                    if (chunkResults.Count < maxResultCount - 64)
                    {
                        if (pendingLoadChunkPos.Count >= 64)
                        {
                            count64++;
                            Parallel.Invoke(actions64);
                            continue;
                        }
                        else if (pendingLoadChunkPos.Count >= 16)
                        {
                            count16++;
                            Parallel.Invoke(actions16);
                            continue;
                        }
                        else if(pendingLoadChunkPos.Count >= 4)
                        {
                            count4++;
                            Parallel.Invoke(actions4);
                            continue;
                        }
                        else if (pendingLoadChunkPos.Count > 0)
                        {
                            count++;
                            action();
                            continue;
                        }
                    }
                    Thread.Sleep(taskSleepMilliseconds);
                }
            }
            catch (Exception e)
            {
                LogUtil.Error(e);
                throw e;
            }
            finally
            {
                LogUtil.Debug($"count64 = {count64}, count16 = {count16}, count4 = {count4}, count = {count}, total = {count64 * 64 + count16 * 16 + count4 * 4 + count}");
            }
        }

        public ChunkProvider(ILevelGenerator generator, LevelDataStorage storage, int maxResultCount)
        {
            this.generator = generator;
            this.storage = storage;
            this.maxResultCount = maxResultCount;
            workerTask = new Task(WorkerRun, TaskCreationOptions.LongRunning);
            taskSleepMilliseconds = 20;
        }

        private readonly ILevelGenerator generator;

        private readonly LevelDataStorage storage;

        private readonly ConcurrentQueue<ChunkPos> pendingLoadChunkPos = new ConcurrentQueue<ChunkPos>();

        private readonly ConcurrentQueue<Chunk> chunkResults = new ConcurrentQueue<Chunk>();

        private readonly int maxResultCount;

        private readonly int taskSleepMilliseconds;

        private readonly Task workerTask;

        private volatile bool stop = false;

    }
}
