using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

using StarCube.Game.Levels;
using StarCube.Game.Levels.Chunks;
using StarCube.Game.Levels.Chunks.Loading;
using StarCube.Game.Levels.Chunks.Storage;
using StarCube.Game.Levels.Generation;
using StarCube.Game.Levels.Storage;
using StarCube.Server.Game.Worlds;

namespace StarCube.Server.Game.Levels
{
    internal sealed class LevelSpawnTask
    {
        public void Done(ServerLevel level)
        {
            this.level = level;
            task.RunSynchronously();
            task.Wait();
        }

        public LevelSpawnTask(Guid guid, Action<ServerLevel>? initializer)
        {
            task = new Task<ServerLevel>(() => level!);
            this.guid = guid;
            this.initializer = initializer;
        }

        public readonly Task<ServerLevel> task;

        public readonly Guid guid;

        public readonly Action<ServerLevel>? initializer;

        private volatile ServerLevel? level;
    }

    internal sealed class LevelLoadTask
    {
        public void Done(ServerWorld? world)
        {
            this.world = world;
            task.RunSynchronously();
            task.Wait();
        }

        public LevelLoadTask(Guid guid)
        {
            this.guid = guid;
            task = new Task<ServerWorld?>(() => world);
        }

        public readonly Guid guid;

        public readonly Task<ServerWorld?> task;

        private volatile ServerWorld? world;
    }

    internal sealed class LevelUnloadTask
    {
        public void Done(bool result)
        {
            this.result = result;
            task.RunSynchronously();
            task.Wait();
        }

        public LevelUnloadTask(Guid guid)
        {
            this.guid = guid;
            task = new Task<bool>(() => result);
        }

        public readonly Guid guid;

        public readonly Task<bool> task;

        private volatile bool result;
    }

    internal sealed class LevelDropTask
    {
        public void Done(bool result)
        {
            this.result = result;
            task.RunSynchronously();
            task.Wait();
        }

        public LevelDropTask(Guid guid)
        {
            task = new Task<bool>(() => result);
            this.guid = guid;
        }

        public readonly Guid guid;

        public readonly Task<bool> task;

        private volatile bool result;
    }

    public sealed class ServerLevelManager : IDisposable
    {
        public IChunkFactory ChunkFactory => storage.chunkFactory;

        public IChunkParser ChunkParser => storage.chunkParser;

        /// <summary>
        /// 异步创建一个 Chunked Level
        /// </summary>
        /// <param name="bounding"></param>
        /// <param name="generator"></param>
        /// <param name="staticAnchors"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<ServerLevel> CreateChunkedAsync(ILevelBounding bounding, ILevelChunkGenerator generator, ServerWorld world, List<AnchorData>? staticAnchors = null)
        {
            Guid guid = Guid.NewGuid();
            var levelStorage = storage.OpenOrCreate(guid);
            ChunkedServerLevel level = new ChunkedServerLevel(guid, bounding, world, generator, levelStorage);
            LevelSpawnTask task = new LevelSpawnTask(guid, null);
            return task.task;
        }

        /// <summary>
        /// 异步创建一个 Integral Level
        /// </summary>
        /// <param name="bounding"></param>
        /// <param name="generator"></param>
        /// <param name="staticAnchors"></param>
        /// <returns></returns>
        public Task<ServerLevel> CreateIntegralAsync(ILevelBounding bounding, ILevelChunkGenerator generator, ServerWorld world)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 异步将 Level 移动到另一个世界
        /// </summary>
        /// <param name="level"></param>
        /// <param name="targetWorldGuid"></param>
        /// <returns></returns>
        public Task<bool> MoveLevelToWorldAsync(ServerLevel level, Guid targetWorldGuid)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 异步加载一个 Level
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public Task<ServerLevel?> LoadLevelAsync(Guid guid)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 异步卸载一个 Level
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public Task<bool> UnloadLevelAsync(Guid guid)
        {
            LevelUnloadTask task = new LevelUnloadTask(guid);
            pendingInvokeLevelTasks.Enqueue(task);
            return task.task;
        }

        /// <summary>
        /// 异步删除一个 Level
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public Task<bool> DropLevelAsync(Guid guid)
        {
            LevelDropTask task = new LevelDropTask(guid);
            pendingInvokeLevelTasks.Enqueue(task);
            return task.task;
        }

        public void Tick()
        {

        }

        public void Save(bool flush)
        {
            storage.Save();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public ServerLevelManager(ServerGame game)
        {
            this.game = game;
            storage = new LevelStorageManager(game.saves);
        }

        public readonly ServerGame game;

        public readonly LevelStorageManager storage;

        private readonly Dictionary<Guid, ServerLevel> guidToLevel = new Dictionary<Guid, ServerLevel>();

        private readonly Dictionary<Guid, ServerLevel> guidToInitializingLevel = new Dictionary<Guid, ServerLevel>();
        private readonly List<Guid> doneInitLevelCache = new List<Guid>();

        private readonly Dictionary<Guid, ServerLevel> guidToUnloadedLevel = new Dictionary<Guid, ServerLevel>();

        private readonly HashSet<Guid> droppedLevels = new HashSet<Guid>();

        /// <summary>
        /// 等待被执行的 world 生成、加载、卸载、删除任务
        /// </summary>
        private readonly ConcurrentQueue<object> pendingInvokeLevelTasks = new ConcurrentQueue<object>();
    }
}
