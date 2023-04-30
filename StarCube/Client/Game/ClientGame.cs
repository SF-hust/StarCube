using System;
using System.Collections.Generic;
using System.Text;
using StarCube.Client.Game.Worlds;
using StarCube.Client.Network;

namespace StarCube.Client.Game
{
    public abstract class ClientGame
    {
        public ClientWorld? World
        {
            get => world;
            set => world = value;
        }

        public ClientGame(LocalPlayer player)
        {
            this.player = player;
            packetHandler = new ClientPacketHandler(this);
        }

        public readonly LocalPlayer player;

        public readonly ClientPacketHandler packetHandler;

        private ClientWorld? world = null;
    }
}
