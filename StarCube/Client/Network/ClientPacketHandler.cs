using System;
using System.Collections.Generic;
using System.Text;
using StarCube.Client.Game;
using StarCube.Network;
using StarCube.Network.Packets;

namespace StarCube.Client.Network
{
    public sealed class ClientPacketHandler : PacketHandler<ClientPacketHandler>
    {
        public ClientPacketHandler(ClientGame game)
            : base(game.player.connection)
        {
            this.game = game;
        }

        public readonly ClientGame game;
    }
}
