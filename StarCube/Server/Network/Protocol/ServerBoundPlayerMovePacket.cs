using System;
using System.Numerics;

using StarCube.Network.Packets;
using StarCube.Network.ByteBuffer;

namespace StarCube.Server.Network.Protocol
{
    public sealed class ServerBoundPlayerMovePacket
        : Packet<ServerBoundPlayerMovePacket, ServerPlayerPacketHandler>
    {
        public override void Encode(IByteBuffer buffer)
        {
            throw new NotImplementedException();
        }

        public override void Handle(ServerPlayerPacketHandler handler)
        {
            handler.player.position = position;
        }

        public ServerBoundPlayerMovePacket(float x, float y, float z)
        {
            position = new Vector3(x, y, z);
        }

        public readonly Vector3 position;
    }
}
