using System;

using StarCube.Network.Packets;
using StarCube.Network.ByteBuffer;

namespace StarCube.Client.Network.Protocol
{
    public sealed class ClientBoundEnterGamePacket
        : Packet<ClientBoundEnterGamePacket, ClientPacketHandler>
    {
        public override void Encode(IByteBuffer buffer)
        {
            throw new NotImplementedException();
        }

        public override void Handle(ClientPacketHandler handler)
        {
            throw new NotImplementedException();
        }
    }
}
