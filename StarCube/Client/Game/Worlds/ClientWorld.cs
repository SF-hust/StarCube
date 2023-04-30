using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using StarCube.Client.Game.Levels;
using StarCube.Game;
using StarCube.Game.Entities;
using StarCube.Game.Levels;
using StarCube.Game.Worlds;

namespace StarCube.Client.Game.Worlds
{
    public class ClientWorld : World
    {
        public override bool ClientSide => true;

        public override IEnumerable<Entity> Entities => throw new NotImplementedException();
        public override IEnumerable<Level> Levels => throw new NotImplementedException();
        public override bool TryGetEntity(Guid guid, [NotNullWhen(true)] out Entity? entity)
        {
            throw new NotImplementedException();
        }
        public override bool TryGetLevel(Guid guid, [NotNullWhen(true)] out Level? level)
        {
            throw new NotImplementedException();
        }

        public bool TryGetClientLevel(Guid guid, [NotNullWhen(true)] out ClientLevel? level)
        {
            throw new NotImplementedException();
        }

        public ClientWorld(Guid guid, ClientGame game)
            : base(guid)
        {
            this.game = game;
        }

        public readonly ClientGame game;
    }
}
