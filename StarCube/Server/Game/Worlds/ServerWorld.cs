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


        public IEnumerable<ChunkedServerLevel> ServerLevels => throw new NotImplementedException();
        public override IEnumerable<Level> Levels => throw new NotImplementedException();
        public override IEnumerable<Entity> Entities => throw new NotImplementedException();
        public override bool TryGetLevel(Guid guid, [NotNullWhen(true)] out Level? level)
        {
            throw new NotImplementedException();
        }
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
            if (tickCount % 100 == 0)
            {
                LogUtil.Debug($"server world ticking (guid = {guid}, tick = {totalTickCount})...");
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

        private bool released = false;
    }
}
