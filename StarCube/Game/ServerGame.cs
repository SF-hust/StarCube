using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using StarCube.Utility.Logging;
using StarCube.Data.Storage;
using StarCube.Game.Blocks;
using StarCube.Game.Levels;
using StarCube.Game.Levels.Chunks;
using StarCube.Game.Levels.Chunks.Storage;
using StarCube.Game.Levels.Chunks.Storage.Palette;
using StarCube.Game.Levels.Generation;
using StarCube.Game.Levels.Storage;
using StarCube.Game.Worlds;

namespace StarCube.Game
{
    public class ServerGame
    {
        private readonly Lazy<Thread> serverGameThread = new Lazy<Thread>(() => Thread.CurrentThread, true);

        /// <summary>
        /// 游戏主线程，这个线程不是进程的主线程，也不是每个 World 的主线程
        /// </summary>
        public Thread ServerGameThread => serverGameThread.IsValueCreated ? serverGameThread.Value : throw new NullReferenceException("ServerGameThread");

        /// <summary>
        /// 启动游戏
        /// </summary>
        public void Start()
        {
            serverGameTask.Start();
        }

        /// <summary>
        /// 暂停游戏，等待所有 World 此刻的更新，并在所有 World 本 Tick 更新完毕后停止游戏
        /// </summary>
        public void Pause()
        {
        }

        /// <summary>
        /// 恢复游戏
        /// </summary>
        public void Resume()
        {
        }

        /// <summary>
        /// 停止游戏，并保存所有游戏内容
        /// </summary>
        public void Stop()
        {
        }



        private void Run()
        {
            _ = serverGameThread.Value;
            try
            {
                world.SpawnLevel(bound, generator);
                Stopwatch stopwatch = new Stopwatch();
                while (true)
                {
                    stopwatch.Restart();
                    world.Tick();
                    stopwatch.Stop();

                    int ms = (int)stopwatch.ElapsedMilliseconds;
                    tickCount++;
                    if (ms < 50)
                    {
                        Thread.Sleep(50 - ms);
                    }
                    else
                    {
                        LogUtil.Warning($"server game tick(at {tickCount}tick) takes {ms}ms");
                    }
                }
            }
            catch(Exception e)
            {
                LogUtil.Fatal(e);
                throw e;
            }
            finally
            {

            }
        }

        private ServerWorld LoadOrCreateWorld()
        {
            return new ServerWorld(Guid.NewGuid(), this);
        }

        private LevelStorageManager CreateLevelStorageManager()
        {
            IChunkFactory chunkFactory = new PalettedChunkFactory(BlockState.GlobalBlockStateIDMap);
            GlobalPaletteManager<BlockState> blockStateGlobalPaletteManager = new GlobalPaletteManager<BlockState>(BlockState.GlobalBlockStateIDMap, BlockState.ToBson);
            IChunkParser chunkParser = new PalettedChunkParser(chunkFactory, blockStateGlobalPaletteManager);
            LevelStorageManager levelStorageManager = new LevelStorageManager(saves, chunkParser);
            return levelStorageManager;
        }

        public ServerGame(GameSaves saves, ILevelBound bound, ILevelGenerator generator)
        {
            serverGameTask = new Task(Run, TaskCreationOptions.LongRunning);
            this.saves = saves;
            world = LoadOrCreateWorld();

            levelStorageManager = CreateLevelStorageManager();
            this.bound = bound;
            this.generator = generator;
        }


        private readonly Task serverGameTask;

        public readonly GameSaves saves;

        public readonly ServerWorld world;

        public readonly LevelStorageManager levelStorageManager;

        private readonly ILevelBound bound;

        private readonly ILevelGenerator generator;

        private ulong tickCount = 0;
    }
}
