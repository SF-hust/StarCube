using System;
using System.Numerics;

using StarCube.Game.Players;
using StarCube.Network;
using StarCube.Server.Game.Worlds;

namespace StarCube.Server.Game
{
    public class ServerPlayer : Player
    {
        public override bool ClientSide => false;

        public ServerPlayer(Guid guid, Connection connection, ServerGame game)
            : base(guid, connection)
        {
            this.game = game;
        }

        public readonly ServerGame game;

        public ServerWorld? world = null;

        public Vector3 position = Vector3.Zero;
    }
}
