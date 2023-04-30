using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using StarCube.Game.Levels;
using StarCube.Game.Levels.Chunks;
using StarCube.Game.Levels.Chunks.Loading;
using StarCube.Game.Levels.Chunks.Storage;
using StarCube.Game.Levels.Generation;
using StarCube.Game.Levels.Storage;
using StarCube.Server.Game;

namespace StarCube.Server.Game.Levels
{
    public sealed class ServerLevelManager
    {
        public IChunkFactory ChunkFactory => storage.chunkFactory;

        public IChunkParser ChunkParser => storage.chunkParser;

        public Task<ChunkedServerLevel> CreateChunkedAsync(ILevelBounding bounding, ILevelChunkGenerator generator, List<AnchorData>? staticAnchors = null)
        {
            throw new NotImplementedException();
        }

        public Task<bool> MoveLevelToWorldAsync(ServerLevel level, Guid targetWorldGuid)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UnloadLevelAsync(Guid guid)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DropLevelAsync(Guid guid)
        {
            throw new NotImplementedException();
        }

        public Task<ServerLevel?> LoadLevelAsync(Guid guid)
        {
            throw new NotImplementedException();
        }

        public void Tick()
        {

        }

        public void Save(bool flush)
        {
            storage.Save();
        }

        public ServerLevelManager(ServerGame game)
        {
            this.game = game;
            storage = new LevelStorageManager(game.saves);
        }

        public readonly ServerGame game;

        public readonly LevelStorageManager storage;
    }
}
