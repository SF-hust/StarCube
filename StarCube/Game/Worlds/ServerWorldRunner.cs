using System;
using System.Threading;
using System.Threading.Tasks;

using StarCube.Utility.Logging;

namespace StarCube.Game.Worlds
{
    public sealed class ServerWorldRunner
    {
        private readonly Lazy<Thread> serverWorldThread = new Lazy<Thread>(() => Thread.CurrentThread, true);

        /// <summary>
        /// world 运行的 thread
        /// </summary>
        public Thread ServerWorldThread => serverWorldThread.IsValueCreated ? serverWorldThread.Value : throw new NullReferenceException(nameof(ServerWorldThread));

        /// <summary>
        /// 上一次 BeginExcute 中的 action 是否已经执行完毕
        /// </summary>
        public bool DoneAction => actionDoneEvent.IsSet;

        /// <summary>
        /// 让 world 在自己的线程上开始执行给定的 action
        /// </summary>
        public void BeginExcute(Action<ServerWorld> action)
        {
            this.action = action;
            actionStartEvent.Set();
        }

        /// <summary>
        /// 等待上一次 BeginExcute 中的 action 执行完成
        /// </summary>
        public void Wait()
        {
            actionDoneEvent.Wait();
        }

        /// <summary>
        /// Runner 是否已经终止
        /// </summary>
        public bool DoneTerminate => task.IsCompleted;

        /// <summary>
        /// 开始销毁 world 对象
        /// </summary>
        public void BeginTerminate()
        {
            terminate = true;
            actionStartEvent.Set();
        }

        /// <summary>
        /// 等待销毁完成
        /// </summary>
        public void WaitForTerminate()
        {
            task.Wait();
            if (task.IsFaulted)
            {
                LogUtil.Error($"server world runner terminate faultly (guid = {world.guid}), exception :\n{task.Exception?.InnerException}");
            }
        }

        private void Run()
        {
            try
            {
                // 设置当前线程的信息
                Thread.CurrentThread.IsBackground = false;
                Thread.CurrentThread.Name = $"Server World Runner Thread (guid = {world.guid})";

                while (true)
                {
                    // 等待 ServerGame 的事件
                    actionStartEvent.Wait();
                    // 事件开始时可能是要终止 world，此时要退出循环
                    if (terminate)
                    {
                        break;
                    }
                    // 自己把开始事件 reset 掉
                    actionStartEvent.Reset();
                    // 实际执行事件
                    action?.Invoke(world);
                    // 告知 ServerGame 已经执行完成 action
                    actionDoneEvent.Set();
                    // 重置事件，等待下一次 action
                    actionDoneEvent.Reset();
                }
            }
            catch (Exception e)
            {
                LogUtil.Fatal($"an exception in world runner (guid = {world.guid}), exception :\n{e}");
                throw;
            }
            finally
            {
                Terminate();
            }
        }


        private void Terminate()
        {
            LogUtil.Debug($"server world Runner (guid = {world.guid}) terminating...");
            world.Dispose();
            actionStartEvent.Dispose();
            actionDoneEvent.Dispose();
            LogUtil.Debug($"server world Runner (guid = {world.guid}) terminate successfully");
        }


        public ServerWorldRunner(ServerWorld world)
        {
            actionStartEvent = new ManualResetEventSlim(false, 0);
            actionDoneEvent = new ManualResetEventSlim(false, 0);
            this.world = world;
            task = new Task(Run, TaskCreationOptions.LongRunning);
            task.Start();
        }

        public readonly ServerWorld world;

        private readonly Task task;

        private readonly ManualResetEventSlim actionStartEvent;

        private readonly ManualResetEventSlim actionDoneEvent;

        private volatile Action<ServerWorld> action = ServerWorldActions.Init;

        private volatile bool terminate = false;
    }
}
