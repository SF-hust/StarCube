using System;
using System.Collections.Concurrent;
using System.Threading;
using StarCube.Game.Levels.Generation;
using StarCube.Game.Levels.Storage;
using StarCube.Game.Worlds;

namespace StarCube.Game.Levels
{
    public abstract class ServerLevel : Level
    {
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

        public sealed override World World
        {
            get => ServerWorld;
            set
            {
                if (!(value is ServerWorld serverWorld))
                {
                    throw new ArgumentException("world is not server world", nameof(value));
                }

                ServerWorld = serverWorld;
            }
        }

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

        public void Save()
        {
            if (saving)
            {
                throw new InvalidOperationException("is saving");
            }

            saving = true;
            DoSave();
            saving = false;
        }

        protected abstract void DoSave();

        public virtual void Release()
        {
            storage.Dispose();
        }

        public ServerLevel(Guid guid, ILevelBounding bounding, ServerWorld world, ILevelChunkGenerator generator, LevelStorage storage)
            : base(guid, bounding)
        {
            game = world.game;
            this.world = world;
            this.generator = generator;
            this.storage = storage;
        }

        public readonly ServerGame game;

        protected ServerWorld? world;

        protected readonly ILevelChunkGenerator generator;

        protected readonly LevelStorage storage;

        private readonly ConcurrentQueue<Action<ServerLevel>> actionQueue = new ConcurrentQueue<Action<ServerLevel>>();

        protected volatile bool active = false;

        protected volatile bool saving = false;
    }
}
