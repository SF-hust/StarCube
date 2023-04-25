using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Linq;

using StarCube.Game.Worlds.Storage;

namespace StarCube.Game.Worlds
{
    internal sealed class WorldSpawnTask
    {
        public void Done(ServerWorld world)
        {
            this.world = world;
            task.RunSynchronously();
            task.Wait();
        }

        public WorldSpawnTask(Guid guid, Action<ServerWorld>? initializer)
        {
            task = new Task<ServerWorld>(() => world!);
            this.guid = guid;
            this.initializer = initializer;
        }

        public readonly Task<ServerWorld> task;

        public readonly Guid guid;

        public readonly Action<ServerWorld>? initializer;

        private volatile ServerWorld? world;
    }

    internal sealed class WorldLoadTask
    {
        public void Done(ServerWorld? world)
        {
            this.world = world;
            task.RunSynchronously();
            task.Wait();
        }

        public WorldLoadTask(Guid guid)
        {
            this.guid = guid;
            task = new Task<ServerWorld?>(() => world);
        }

        public readonly Guid guid;

        public readonly Task<ServerWorld?> task;

        private volatile ServerWorld? world;
    }

    internal sealed class WorldUnloadTask
    {
        public void Done(bool result)
        {
            this.result = result;
            task.RunSynchronously();
            task.Wait();
        }

        public WorldUnloadTask(Guid guid)
        {
            this.guid = guid;
            task = new Task<bool>(() => result);
        }

        public readonly Guid guid;

        public readonly Task<bool> task;

        private volatile bool result;
    }

    internal sealed class WorldDropTask
    {
        public void Done(bool result)
        {
            this.result = result;
            task.RunSynchronously();
            task.Wait();
        }

        public WorldDropTask(Guid guid)
        {
            task = new Task<bool>(() => result);
            this.guid = guid;
        }

        public readonly Guid guid;

        public readonly Task<bool> task;

        private volatile bool result;
    }


    public sealed class ServerWorldManager : IDisposable
    {
        /// <summary>
        /// 初始化 ServerWorldManager
        /// </summary>
        public void Init()
        {
            // 如果游戏是新创建的，生成一个 world
            if (game.TotalTickCount == 0L)
            {
                Guid guid = Guid.NewGuid();
                ServerWorldStorage worldStorage = storage.OpenOrCreate(guid);
                ServerWorld world = new ServerWorld(guid, game, worldStorage);
                ServerWorldRunner runner = new ServerWorldRunner(world);
                guidToServerWorldRunner.Add(guid, runner);
            }
            // 游戏不是新创建的，从存档中加载活跃的 world
            else
            {
                List<Guid> activeWorldGuidList = storage.LoadActiveWorldList();
                foreach (Guid guid in activeWorldGuidList)
                {
                    ServerWorldStorage worldStorage = storage.OpenOrCreate(guid);
                    ServerWorld world = new ServerWorld(guid, game, worldStorage);
                    ServerWorldRunner runner = new ServerWorldRunner(world);
                    guidToServerWorldRunner.Add(guid, runner);
                }
            }

            // 对每个 world 执行初始化并等待执行完毕
            foreach (var runner in guidToServerWorldRunner.Values)
            {
                runner.BeginExcute(ServerWorldActions.Init);
            }
            foreach (var runner in guidToServerWorldRunner.Values)
            {
                runner.Wait();
            }
        }

        public void Tick()
        {
            // tick 所有活跃的 world
            foreach (var runner in guidToServerWorldRunner.Values)
            {
                runner.BeginExcute(ServerWorldActions.Tick);
            }
            // 等待活跃的 world 停止 tick
            foreach (var runner in guidToServerWorldRunner.Values)
            {
                runner.Wait();
            }

            // 处理各个 world task
            while (pendingInvokeWorldTasks.TryDequeue(out var worldTask))
            {
                if (worldTask is WorldSpawnTask spawnTask)
                {
                    ServerWorldStorage worldStorage = storage.OpenOrCreate(spawnTask.guid);
                    ServerWorld world = new ServerWorld(spawnTask.guid, game, worldStorage, spawnTask.initializer);
                    ServerWorldRunner runner = new ServerWorldRunner(world);
                    runner.BeginExcute(ServerWorldActions.Init);
                    guidToInitializingWorld.Add(spawnTask.guid, (runner, spawnTask));
                }
                else if (worldTask is WorldLoadTask loadTask)
                {
                    // 如果 world 已被标记为销毁，返回 null
                    if (droppedWorldGuid.Contains(loadTask.guid))
                    {
                        loadTask.Done(null);
                        continue;
                    }

                    // 如果 world 已被标记卸载，但尚未执行卸载，将其重新设为活跃
                    if (guidToUnloadedWorldRunner.Remove(loadTask.guid, out var runner))
                    {
                        // 重置 world 的 tick count
                        runner.BeginExcute(ServerWorldActions.Reset);
                        runner.Wait();

                        guidToServerWorldRunner.Add(loadTask.guid, runner);
                        loadTask.Done(runner.world);
                        continue;
                    }

                    // 如果 world 已经被加载或正在初始化中，或者不存在对应的存档，返回 null
                    if (guidToServerWorldRunner.ContainsKey(loadTask.guid) ||
                        guidToInitializingWorld.ContainsKey(loadTask.guid) ||
                        !storage.Contains(loadTask.guid))
                    {
                        loadTask.Done(null);
                        continue;
                    }

                    // 创建 world 并从存档中加载
                    ServerWorldStorage worldStorage = storage.OpenOrCreate(loadTask.guid);
                    ServerWorld world = new ServerWorld(loadTask.guid, game, worldStorage);
                    runner = new ServerWorldRunner(world);
                    runner.BeginExcute(ServerWorldActions.Init);
                    guidToInitializingWorld.Add(loadTask.guid, (runner, loadTask));
                    loadTask.Done(world);
                }
                else if (worldTask is WorldUnloadTask unloadTask)
                {
                    // 如果 world 没有被加载，返回 false
                    if (!guidToServerWorldRunner.Remove(unloadTask.guid, out var runner))
                    {
                        unloadTask.Done(false);
                        continue;
                    }

                    guidToUnloadedWorldRunner.Add(unloadTask.guid, runner);
                    unloadTask.Done(true);
                }
                else if (worldTask is WorldDropTask dropTask)
                {
                    // 如果 world 已被加载，或正在初始化，或已经被标记销毁，或不存在，返回 false
                    if (guidToServerWorldRunner.ContainsKey(dropTask.guid) ||
                        guidToInitializingWorld.ContainsKey(dropTask.guid) ||
                        droppedWorldGuid.Contains(dropTask.guid) ||
                        !storage.Contains(dropTask.guid))
                    {
                        dropTask.Done(false);
                        continue;
                    }

                    droppedWorldGuid.Add(dropTask.guid);
                    dropTask.Done(true);
                }
            }

            // 处理初始化完成的 world
            foreach (var tuple in guidToInitializingWorld.Values)
            {
                var runner = tuple.Item1;
                if (runner.DoneAction)
                {
                    doneInitWorldList.Add(runner.world.guid);
                }
            }
            foreach (Guid guid in doneInitWorldList)
            {
                guidToInitializingWorld.Remove(guid, out var tuple);
                guidToServerWorldRunner.Add(guid, tuple.Item1);
                var runner = tuple.Item1;
                if (tuple.Item2 is WorldSpawnTask spawnTask)
                {
                    spawnTask.Done(runner.world);
                }
                else if (tuple.Item2 is WorldLoadTask loadTask)
                {
                    loadTask.Done(runner.world);
                }
                else
                {
                    throw new InvalidCastException("world task");
                }
            }
            doneInitWorldList.Clear();
        }

        /// <summary>
        /// 保存 world 的元信息以及所有的 world
        /// </summary>
        /// <param name="flush"> 是否要刷新所有初始化中的 world </param>
        public void Save(bool flush)
        {
            if (flush)
            {
                // 刷新所有正在初始化的 world
                foreach (var tuple in guidToInitializingWorld.Values)
                {
                    var runner = tuple.Item1;
                    runner.Wait();
                    guidToServerWorldRunner.Add(runner.world.guid, runner);
                    if (tuple.Item2 is WorldSpawnTask spawnTask)
                    {
                        spawnTask.Done(runner.world);
                    }
                    else if (tuple.Item2 is WorldLoadTask loadTask)
                    {
                        loadTask.Done(runner.world);
                    }
                    else
                    {
                        throw new InvalidCastException("world task");
                    }
                }
                guidToInitializingWorld.Clear();
            }

            // 清理所有被卸载的 world
            foreach (var runner in guidToUnloadedWorldRunner.Values)
            {
                runner.BeginTerminate();
            }
            foreach (var runner in guidToUnloadedWorldRunner.Values)
            {
                runner.WaitForTerminate();
            }
            guidToUnloadedWorldRunner.Clear();

            // 销毁所有待销毁的 world
            foreach (Guid guid in droppedWorldGuid)
            {
                storage.Drop(guid);
            }
            droppedWorldGuid.Clear();

            // 保存所有活跃的 world
            foreach (var runner in guidToServerWorldRunner.Values)
            {
                runner.BeginExcute(ServerWorldActions.Save);
            }
            foreach (var runner in guidToServerWorldRunner.Values)
            {
                runner.Wait();
            }

            // 保存 world 元数据
            storage.Save();
        }

        public bool TryGet(Guid guid, [NotNullWhen(true)] out ServerWorld? world)
        {
            if(guidToServerWorldRunner.TryGetValue(guid, out var runner))
            {
                world = runner.world;
                return true;
            }

            world = null;
            return false;
        }

        public IEnumerable<ServerWorld> ServerWorlds => guidToServerWorldRunner.Values.Select((runner) => runner.world);

        /// <summary>
        /// 请求生成一个 ServerWorld
        /// </summary>
        /// <param name="initializer"></param>
        /// <returns></returns>
        public Task<ServerWorld> Spawn(Action<ServerWorld>? initializer = null)
        {
            WorldSpawnTask spawnTask = new WorldSpawnTask(Guid.NewGuid(), initializer);
            pendingInvokeWorldTasks.Enqueue(spawnTask);
            return spawnTask.task;
        }

        /// <summary>
        /// 请求加载一个 ServerWorld
        /// </summary>
        /// <param name="guid"></param>
        /// <returns> 成功加载且已被放进 manager 中的 ServerWorld，或加载失败时为 null </returns>
        public Task<ServerWorld?> Load(Guid guid)
        {
            WorldLoadTask loadTask = new WorldLoadTask(guid);
            pendingInvokeWorldTasks.Enqueue(loadTask);
            return loadTask.task;
        }

        /// <summary>
        /// 请求卸载一个 ServerWorld
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public Task<bool> Unload(Guid guid)
        {
            WorldUnloadTask unloadTask = new WorldUnloadTask(guid);
            pendingInvokeWorldTasks.Enqueue(unloadTask);
            return unloadTask.task;
        }

        /// <summary>
        /// 请求删除掉一个 ServerWorld
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public Task<bool> Drop(Guid guid)
        {
            WorldDropTask dropTask = new WorldDropTask(guid);
            pendingInvokeWorldTasks.Enqueue(dropTask);
            return dropTask.task;
        }

        public void Dispose()
        {
            // 清理初始化中的 world
            foreach (var tuple in guidToInitializingWorld.Values)
            {
                tuple.Item1.Wait();
            }
            foreach (var tuple in guidToInitializingWorld.Values)
            {
                tuple.Item1.BeginTerminate();
            }
            foreach (var tuple in guidToInitializingWorld.Values)
            {
                tuple.Item1.WaitForTerminate();
            }
            guidToInitializingWorld.Clear();

            // 清理已卸载的 world
            foreach (var runner in guidToUnloadedWorldRunner.Values)
            {
                runner.BeginTerminate();
            }
            foreach (var runner in guidToUnloadedWorldRunner.Values)
            {
                runner.WaitForTerminate();
            }
            guidToUnloadedWorldRunner.Clear();

            // 清理活跃的 world
            foreach (var runner in guidToServerWorldRunner.Values)
            {
                runner.BeginTerminate();
            }
            foreach (var runner in guidToServerWorldRunner.Values)
            {
                runner.WaitForTerminate();
            }
            guidToServerWorldRunner.Clear();

            storage.Dispose();
        }

        public ServerWorldManager(ServerGame game)
        {
            this.game = game;
            storage = new ServerWorldStorageManager(game.saves);
        }

        public readonly ServerGame game;

        public readonly ServerWorldStorageManager storage;

        /// <summary>
        /// 活跃的 world，会参与每次 tick 与 save
        /// </summary>
        private readonly Dictionary<Guid, ServerWorldRunner> guidToServerWorldRunner = new Dictionary<Guid, ServerWorldRunner>();

        /// <summary>
        /// 加载或生成中的 world，不会参与 tick 与 save
        /// </summary>
        private readonly Dictionary<Guid, (ServerWorldRunner, object)> guidToInitializingWorld = new Dictionary<Guid, (ServerWorldRunner, object)>();

        private readonly List<Guid> doneInitWorldList = new List<Guid>();

        /// <summary>
        /// 已卸载的 world，等待下一次存档时被释放
        /// </summary>
        private readonly Dictionary<Guid, ServerWorldRunner> guidToUnloadedWorldRunner = new Dictionary<Guid, ServerWorldRunner>();

        /// <summary>
        /// 已销毁的 world 的 Guid，等待下一次存档时被彻底删除
        /// </summary>
        private readonly HashSet<Guid> droppedWorldGuid = new HashSet<Guid>();

        /// <summary>
        /// 等待被执行的 world 生成、加载、卸载、删除任务
        /// </summary>
        private readonly ConcurrentQueue<object> pendingInvokeWorldTasks = new ConcurrentQueue<object>();
    }
}
