using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using StarCube.Game.Entities;
using StarCube.Game.Levels;
using StarCube.Game.Levels.Generation;
using StarCube.Game.Levels.Storage;
using StarCube.Game.Ticking;

namespace StarCube.Game.Worlds
{
    public class ServerWorld : World, ITickable
    {
        public IEnumerable<ServerLevel> ServerLevels => idToServerLevel.Values;

        public override IEnumerable<Level> Levels => idToServerLevel.Values;

        public override IEnumerable<Entity> Entities => throw new NotImplementedException();

        public override bool TryGetLevel(Guid guid, [NotNullWhen(true)] out Level? level)
        {
            if (idToServerLevel.TryGetValue(guid, out ServerLevel? serverLevel))
            {
                level = serverLevel;
                return true;
            }

            level = null;
            return false;
        }

        public void Tick()
        {
            foreach (ServerLevel level in idToServerLevel.Values)
            {
                level.Tick();
            }
        }

        public ServerLevel SpawnLevel(ILevelBound bound, ILevelGenerator generator)
        {
            Guid guid = Guid.NewGuid();
            LevelStorage storage = game.levelStorageManager.GetOrCreate(guid);
            ServerLevel level = new ServerLevel(guid, bound, generator, storage);
            return level;
        }

        public override bool TryGetEntity(Guid guid, [NotNullWhen(true)] out Entity? entity)
        {
            throw new NotImplementedException();
        }

        public ServerWorld(Guid guid, ServerGame game) : base(guid, false)
        {
            this.game = game;
        }

        public readonly ServerGame game;

        private readonly Dictionary<Guid, ServerLevel> idToServerLevel = new Dictionary<Guid, ServerLevel>();
    }
}
