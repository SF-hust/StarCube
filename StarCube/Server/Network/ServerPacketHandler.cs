using StarCube.Network;
using StarCube.Network.Packets;
using StarCube.Server.Game;

namespace StarCube.Server.Network
{
    public sealed class ServerPacketHandler
        : PacketHandler<ServerPacketHandler>
    {
        public ServerPacketHandler(Connection connection, ServerGame game)
            : base(connection)
        {
            this.game = game;
        }

        public readonly ServerGame game;

    }
}
