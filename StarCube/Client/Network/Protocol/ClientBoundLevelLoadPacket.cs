using System;

using StarCube.Network.Packets;
using StarCube.Network.ByteBuffer;

namespace StarCube.Client.Network.Protocol
{
    public sealed class ClientBoundLevelLoadPacket
        : Packet<ClientBoundLevelLoadPacket, ClientPacketHandler>
    {
        public override void Encode(IByteBuffer buffer)
        {
        }

        public override void Handle(ClientPacketHandler handler)
        {
        }

        public readonly Guid guid;

        public readonly int bottom;

        public readonly int height;

        public readonly int radius;
    }
}
