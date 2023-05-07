using StarCube.Network.Packets;
using StarCube.Client.Game;

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
