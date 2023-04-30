using StarCube.Network;
using StarCube.Network.Packets;
using StarCube.Server.Game;

namespace StarCube.Server.Network
{
    public sealed class ServerPlayerPacketHandler
        : PacketHandler<ServerPlayerPacketHandler>
    {
        public ServerPlayerPacketHandler(Connection connection, ServerPlayer player)
            : base(connection)
        {
            this.player = player;
        }

        public readonly ServerPlayer player;
    }
}
