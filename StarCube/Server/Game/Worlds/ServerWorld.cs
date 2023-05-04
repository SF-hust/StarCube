using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

using StarCube.Utility.Logging;
using StarCube.Game.Entities;
using StarCube.Game.Levels;
using StarCube.Game.Worlds.Storage;
using StarCube.Game.Worlds;
using StarCube.Server.Game.Levels;

namespace StarCube.Server.Game.Worlds
{
    public sealed class ServerWorld : World, IDisposable
    {
        public override bool ClientSide => false;

        /// <summary>
        /// world 运行的总 tick 计数
        /// </summary>
        public long TotalTickCount => Interlocked.Read(ref totalTickCount);

        /// <summary>
        /// 自本次加载以来，world 运行的总 tick 计数
        /// </summary>
        public long TickCount => Interlocked.Read(ref tickCount);


        public IEnumerable<ServerLevel> ServerLevels => guidToLevel.Values;

        public override IEnumerable<Level> Levels => guidToLevel.Values;

        public bool TryGetServerLevel(Guid guid, [NotNullWhen(true)] out ServerLevel? level)
        {
            return guidToLevel.TryGetValue(guid, out level);
        }

        public override bool TryGetLevel(Guid guid, [NotNullWhen(true)] out Level? level)
        {
            if (TryGetServerLevel(guid, out ServerLevel? serverLevel))
            {
                level = serverLevel;
                return true;
            }

            level = null;
            return false;
        }


        public override IEnumerable<Entity> Entities => throw new NotImplementedException();
        public override bool TryGetEntity(Guid guid, [NotNullWhen(true)] out Entity? entity)
        {
            throw new NotImplementedException();
        }


        public void Init()
        {
            totalTickCount = storage.LoadTotalTickCount();

            intializer?.Invoke(this);
        }

        public void Tick()
        {
            // 更新所有 level
            foreach (ServerLevel level in guidToLevel.Values)
            {
                level.Tick();
            }

            // 更新 tickCount
            Interlocked.Increment(ref tickCount);
            Interlocked.Increment(ref totalTickCount);
        }

        public void Reset()
        {
            Interlocked.Exchange(ref tickCount, 0L);
        }

        public void Save()
        {
            LogUtil.Debug($"server world saving (Guid = {guid}, Tick = {totalTickCount})...");

            // 保存 world 的 totalTickCount
            storage.SaveTotalTickCount(totalTickCount);
        }


        /// <summary>
        /// 向世界中添加一个 Level，只能在游戏主线程调用
        /// </summary>
        /// <param name="level"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void AddLevel(ServerLevel level)
        {
            if (Thread.CurrentThread != game.ServerGameThread)
            {
                throw new InvalidOperationException("only called on ServerGameThread");
            }

            guidToLevel.Add(level.guid, level);
            level.OnAddToWorld(this);
            level.Active = true;
        }

        /// <summary>
        /// 从世界中移除一个 Level，只能在游戏主线程调用
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public bool RemoveLevel(Guid guid, [NotNullWhen(true)] out ServerLevel? level)
        {
            if (Thread.CurrentThread != game.ServerGameThread)
            {
                throw new InvalidOperationException("only called on ServerGameThread");
            }

            if (guidToLevel.TryGetValue(guid, out level))
            {
                level.Active = false;
                level.OnRemoveFromWorld();
                guidToLevel.Remove(guid);
                return true;
            }

            return false;
        }


        public void Dispose()
        {
            if (released)
            {
                throw new ObjectDisposedException(nameof(ServerWorld), "double release");
            }

            storage.Release();

            released = true;
        }

        public ServerWorld(Guid guid, ServerGame game, WorldStorage storage, Action<ServerWorld>? intializer = null) : base(guid)
        {
            this.game = game;
            this.storage = storage;
            this.intializer = intializer;
        }

        public readonly ServerGame game;

        public readonly WorldStorage storage;

        private readonly Action<ServerWorld>? intializer;

        private long totalTickCount = 0L;

        private long tickCount = 0L;

        private readonly Dictionary<Guid, ServerLevel> guidToLevel = new Dictionary<Guid, ServerLevel>();

        private bool released = false;
    }
}
