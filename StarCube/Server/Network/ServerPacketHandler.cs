using System;
using System.Collections.Generic;
using System.Text;

using StarCube.Game;
using StarCube.Network;
using StarCube.Network.Packets;
using StarCube.Server.Game;

namespace StarCube.Server.Network
{
    public sealed class ServerPacketHandler
    {
        public ServerPacketHandler(ServerGame game)
        {
            this.game = game;
        }

        public readonly ServerGame game;
    }
}
