using System;

using StarCube.Client.Game.Worlds;
using StarCube.Client.Network;

namespace StarCube.Client.Game
{
    public abstract class ClientGame
    {
        public ClientWorld? CurrentWorld => currentWorld;

        public abstract void OnWorldChange(Guid guid, float x, float y, float z);

        public ClientGame(LocalPlayer player)
        {
            this.player = player;
            packetHandler = new ClientPacketHandler(this);
        }

        public readonly LocalPlayer player;

        public readonly ClientPacketHandler packetHandler;

        protected ClientWorld? currentWorld = null;
    }
}
