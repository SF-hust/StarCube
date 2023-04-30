using System;

using StarCube.Network.Packets;
using StarCube.Network.ByteBuffer;

namespace StarCube.Server.Network.Protocol
{
    public sealed class ServerBoundPlayerEnterPacket
        : Packet<ServerBoundPlayerEnterPacket, ServerPacketHandler>
    {
        public override void Encode(IByteBuffer buffer)
        {
            throw new NotImplementedException();
        }

        public override void Handle(ServerPacketHandler handler)
        {
        }

        public ServerBoundPlayerEnterPacket(string name)
        {
            this.name = name;
        }

        public readonly string name;
    }
}
