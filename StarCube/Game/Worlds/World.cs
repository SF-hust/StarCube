using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using StarCube.Utility;
using StarCube.Game.Levels;
using StarCube.Game.Entities;

namespace StarCube.Game.Worlds
{
    public abstract class World
        : IGuid
    {
        public abstract bool ClientSize { get; }

        public abstract IEnumerable<Entity> Entities { get; }

        public abstract bool TryGetEntity(Guid guid, [NotNullWhen(true)] out Entity? entity);

        public abstract IEnumerable<Level> Levels { get; }

        public abstract bool TryGetLevel(Guid guid, [NotNullWhen(true)] out Level? level);


        Guid IGuid.Guid => guid;

        public World(Guid guid)
        {
            this.guid = guid;
        }

        public readonly Guid guid;
    }
}
