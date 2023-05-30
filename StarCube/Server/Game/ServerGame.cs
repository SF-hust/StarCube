using System;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;

using StarCube.Utility.Logging;
using StarCube.Data.Storage;
using StarCube.Game;
using StarCube.Server.Game.Worlds;
using StarCube.Server.Game.Levels;

namespace StarCube.Server.Game
{
    /// <summary>
    /// 表示服务端游戏实例
    /// </summary>
    public sealed class ServerGame
    {
        private readonly Lazy<Thread> serverGameThread = new Lazy<Thread>(() => Thread.CurrentThread, true);

        /// <summary>
        /// 游戏主线程
        /// </summary>
        public Thread ServerGameThread => serverGameThread.IsValueCreated ? serverGameThread.Value : throw new NullReferenceException(nameof(ServerGameThread));

        /// <summary>
        /// 游戏自本次启动后总 Tick 时长
        /// </summary>
        public long TickCount => Interlocked.Read(ref tickCount);

        /// <summary>
        /// 游戏自存档创建以来总 Tick 时长
        /// </summary>
        public long TotalTickCount => Interlocked.Read(ref totalTickCount);

        /// <summary>
        /// 游戏每 tick 所花费的目标时间，取值范围 [0, 2000]
        /// </summary>
        public int MilliSecondsTickTime
        {
            get => millisecondsTickTime;
            set => millisecondsTickTime = Math.Clamp(value, 0, 2000);
        }

        /// <summary>
        /// 上一次保存时游戏的 TickCount
        /// </summary>
        public long LastSaveTickCount => Interlocked.Read(ref lastSaveTickCount);

        /// <summary>
        /// 游戏触发自动保存的间隔，此值小于 0 时禁用自动保存
        /// </summary>
        public int AutoSaveTickCount
        {
            get => autoSaveTickCount;
            set => autoSaveTickCount = value;
        }

        /// <summary>
        /// 当游戏退出时是否要保存游戏
        /// </summary>
        public bool SaveWhenShutdown
        {
            get => saveWhenExit;
            set => saveWhenExit = value;
        }

        /// <summary>
        /// ServerGame 是否正在初始化
        /// </summary>
        public bool Initializing => initializing;

        /// <summary>
        /// ServerGame 是否正处于暂停状态
        /// </summary>
        public bool Paused => paused;

        /// <summary>
        /// ServerGame 是否正在 tick
        /// </summary>
        public bool Ticking => ticking;

        /// <summary>
        /// ServerGame 是否正在进行存档
        /// </summary>
        public bool Saving => saving;

        /// <summary>
        /// ServerGame 是否正在退出
        /// </summary>
        public bool Exiting => exiting;

        /// <summary>
        /// 启动游戏
        /// </summary>
        public void Start()
        {
            serverGameTask.Start();
        }

        /// <summary>
        /// 暂停游戏或取消暂停游戏
        /// </summary>
        /// <param name="pause"></param>
        public void Pause(bool pause)
        {
            shouldPause = pause;
        }

        /// <summary>
        /// 退出游戏，并保存所有游戏内容
        /// </summary>
        public void Exit()
        {
            shouldExit = true;
            // 此时游戏可能已经暂停，解除游戏的暂停状态
            paused = false;
            // 等待游戏线程退出
            serverGameTask.Wait();
            // 如果退出游戏过程中出现异常则打印异常信息
            if (serverGameTask.IsFaulted)
            {
                LogUtil.Fatal($"exception during shuting down server game :\n{serverGameTask.Exception?.InnerException}");
            }
        }

        /// <summary>
        /// 保存游戏并等待游戏保存完毕，只能在游戏暂停时调用
        /// </summary>
        public void Save()
        {
            if (!paused)
            {
                return;
            }

            Task task = new Task(() => SaveGame(true));
            taskQueue.Enqueue(task);
            task.Wait();
        }

        /// <summary>
        /// ServerGame 主线程入口
        /// </summary>
        private void Run()
        {
            // 指示 try {} 块中是否有异常出现
            bool exception = false;
            try
            {
                // 初始化游戏
                InitGame();

                // 如果游戏开始前就被暂停了，等待游戏恢复
                if (shouldPause)
                {
                    LogUtil.Info("server game is paused before game loop start");
                    PauseLoop();
                }

                Stopwatch stopwatch = new Stopwatch();
                LogUtil.Info("server game loop start...");

                // 开始游戏循环
                while (!shouldExit)
                {
                    stopwatch.Restart();

                    // 执行游戏更新
                    TickGame();

                    // 增加 tick 计数
                    Interlocked.Increment(ref tickCount);
                    Interlocked.Increment(ref totalTickCount);

                    // 判断是否需要自动保存游戏
                    bool autoSave = tickCount - lastSaveTickCount >= autoSaveTickCount;

                    // 自动保存游戏
                    if (autoSave && autoSaveTickCount >= 0)
                    {
                        int msBegin = (int)stopwatch.ElapsedMilliseconds;
                        SaveGame(false);
                        int msEnd = (int)stopwatch.ElapsedMilliseconds;
                        LogUtil.Debug($"auto saving server game takes {msEnd - msBegin}ms at {totalTickCount}tick");
                    }

                    stopwatch.Stop();

                    // 记录本次更新花费时间
                    int ms = (int)stopwatch.ElapsedMilliseconds;

                    // 更新花费时间太长，发出警告
                    if (ms > MilliSecondsTickTime)
                    {
                        LogUtil.Warning($"server game tick (tick = {totalTickCount}) takes {ms}ms");
                    }

                    // 检查是否暂停游戏
                    if (shouldPause)
                    {
                        LogUtil.Info($"server game paused (tick = {totalTickCount})...");
                        PauseLoop();
                        LogUtil.Info($"server game resumed (tick = {totalTickCount})");
                        continue;
                    }

                    // 如果没用完一个 tick 的总时间，剩余时间睡过去
                    if (ms < MilliSecondsTickTime)
                    {
                        Thread.Sleep(MilliSecondsTickTime - ms);
                    }
                }

                LogUtil.Info("shutting down server game...");

                exiting = true;

                // 正常结束游戏循环，如果设置了结束后保存游戏，就保存游戏
                if (saveWhenExit)
                {
                    SaveGame(true);
                }
            }
            catch (Exception e)
            {
                // 打印异常
                string state;
                if (initializing)
                {
                    state = nameof(initializing);
                }
                else if (ticking)
                {
                    state = nameof(ticking);
                }
                else if (saving)
                {
                    state = nameof(saving);
                }
                else if (paused)
                {
                    state = nameof(paused);
                }
                else
                {
                    state = "unknown";
                }
                LogUtil.Fatal($"exception in ServerGame, state = {state}, exception :\n{e}");

                initializing = false;
                paused = false;
                ticking = false;
                saving = false;

                exception = true;
                throw;
            }
            finally
            {
                exiting = true;
                Dispose();
                exiting = false;
                if (!exception)
                {
                    LogUtil.Info("server game shutdown with no exception");
                }
                else
                {
                    LogUtil.Warning("server game shutdown for exception");
                }
            }
        }

        /// <summary>
        /// 游戏初始化，在游戏循环开始前会执行一次
        /// </summary>
        private void InitGame()
        {
            LogUtil.Info("initializing server game...");

            initializing = true;

            // 配置游戏主线程
            _ = serverGameThread.Value;
            ServerGameThread.Priority = ThreadPriority.Highest;
            ServerGameThread.IsBackground = false;
            ServerGameThread.Name = "Server Game Thread";

            // 读取游戏总刻数
            totalTickCount = storage.LoadTotalTickCount();

            // 初始化 worlds
            worlds.Init();

            initializing = false;

            LogUtil.Info("initialize server game done");
        }

        /// <summary>
        /// 执行游戏在 1 tick 内的更新
        /// </summary>
        private void TickGame()
        {
            ticking = true;
            // tick 所有的 world
            worlds.Tick();
            ticking = false;
        }

        /// <summary>
        /// 游戏暂停后执行的循环
        /// </summary>
        private void PauseLoop()
        {
            paused = true;
            while(shouldPause)
            {
                while (taskQueue.TryDequeue(out Task? task))
                {
                    task.RunSynchronously();
                    task.Wait();
                }
                if (shouldExit)
                {
                    LogUtil.Info($"server game resume for exit");
                    break;
                }
                Thread.Sleep(50);
            }
            paused = false;
        }

        /// <summary>
        /// 执行游戏保存
        /// </summary>
        /// <param name="flush"> 是否要等待所有后台任务完成，默认为 false，手动保存或退出游戏保存时为 true </param>
        private void SaveGame(bool flush)
        {
            if (lastSaveTickCount == tickCount)
            {
                return;
            }

            if (flush)
            {
                LogUtil.Debug($"saving server game (tick = {totalTickCount})...");
            }
            else
            {
                LogUtil.Debug($"auto saving server game (tick = {totalTickCount})...");
            }

            saving = true;

            levels.Save(flush);
            worlds.Save(flush);
            storage.SaveTotalTickCount(totalTickCount);

            saving = false;

            lastSaveTickCount = tickCount;
        }


        public void OnPlayerEnter(string name)
        {

        }


        private void Dispose()
        {
            if (disposed)
            {
                LogUtil.Error("double dispose ServerGame");
                throw new ObjectDisposedException(nameof(ServerGame));
            }

            worlds.Dispose();
            storage.Dispose();
            saves.Dispose();

            disposed = true;
        }


        public ServerGame(GameSaves saves)
        {
            this.saves = saves;

            storage = new GameStorage(saves);

            players = new ServerPlayerList(this);
            worlds = new ServerWorldManager(this);
            levels = new ServerLevelManager(this);

            serverGameTask = new Task(Run, TaskCreationOptions.LongRunning);
        }

        public readonly GameSaves saves;

        public readonly GameStorage storage;


        public readonly ServerPlayerList players;

        public readonly ServerWorldManager worlds;

        public readonly ServerLevelManager levels;


        private readonly Task serverGameTask;

        private readonly ConcurrentQueue<Task> taskQueue = new ConcurrentQueue<Task>();


        private long tickCount = 0;
        private long totalTickCount = 0;
        private long lastSaveTickCount = 0;

        private volatile int millisecondsTickTime = 50;
        private volatile int autoSaveTickCount = 100;

        private volatile bool initializing = false;
        private volatile bool paused = false;
        private volatile bool ticking = false;
        private volatile bool saving = false;
        private volatile bool exiting = false;
        private volatile bool saveWhenExit = true;
        private volatile bool shouldPause = false;
        private volatile bool shouldExit = false;
        private volatile bool disposed = false;
    }
}
