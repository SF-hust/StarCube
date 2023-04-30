using System;
using System.Collections.Concurrent;
using System.Threading;

using StarCube.Game.Levels;
using StarCube.Game.Levels.Storage;
using StarCube.Game.Worlds;
using StarCube.Server.Game;
using StarCube.Server.Game.Worlds;

namespace StarCube.Server.Game.Levels
{
    public abstract class ServerLevel : Level
    {
        /// <summary>
        /// ServerLevel 的活跃状态
        /// </summary>
        public sealed override bool Active
        {
            get => active;
            set
            {
                if (world == null)
                {
                    throw new InvalidOperationException(nameof(Active));
                }

                active = value;
            }
        }

        public sealed override World World => ServerWorld;

        /// <summary>
        /// ServerLevel 所存在的 ServerWorld
        /// </summary>
        public ServerWorld ServerWorld
        {
            get => world ?? throw new NullReferenceException(nameof(World));
            set
            {
                if (Thread.CurrentThread != game.ServerGameThread)
                {
                    throw new InvalidOperationException("can't set or reset server level's world from other than ServerGameThread");
                }

                if (active)
                {
                    throw new InvalidOperationException("can't set or reset server level's world when active");
                }

                world = value;
            }
        }

        /// <summary>
        /// 向 Level 中加入一个事件，在最近一次 Level 更新末尾被执行
        /// </summary>
        /// <param name="action"></param>
        public void EnqueueLevelUpdate(Action<ServerLevel> action)
        {
            actionQueue.Enqueue(action);
        }

        protected void TickLevelUpdate()
        {
            while (actionQueue.TryDequeue(out var action))
            {
                action(this);
            }
        }

        /// <summary>
        /// 初始化 Level
        /// </summary>
        public abstract void Init();

        /// <summary>
        /// tick Level
        /// </summary>
        public abstract override void Tick();

        /// <summary>
        /// 将 Level 的内容保存到数据库中
        /// </summary>
        /// <param name="flush"></param>
        public abstract void Save(bool flush);

        public virtual void Release()
        {
            storage.Release();
        }

        public ServerLevel(Guid guid, ILevelBounding bounding, ServerWorld world, LevelStorage storage)
            : base(guid, bounding)
        {
            game = world.game;
            this.world = world;
            this.storage = storage;
        }

        public readonly ServerGame game;

        protected volatile ServerWorld? world;

        protected readonly LevelStorage storage;

        private readonly ConcurrentQueue<Action<ServerLevel>> actionQueue = new ConcurrentQueue<Action<ServerLevel>>();

        protected volatile bool active = false;
    }
}
