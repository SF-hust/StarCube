using System;

using StarCube.Game.Players;
using StarCube.Network;

namespace StarCube.Server.Game
{
    public class ServerPlayer : Player
    {
        public override bool ClientSide => throw new NotImplementedException();

        public ServerPlayer(Guid guid, Connection connection, ServerGame game)
            : base(guid, connection)
        {
            this.game = game;
        }

        public readonly ServerGame game;
    }
}
