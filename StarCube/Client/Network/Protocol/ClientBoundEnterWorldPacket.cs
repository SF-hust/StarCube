using System;
using System.Numerics;

using StarCube.Network.Packets;
using StarCube.Network.ByteBuffer;

namespace StarCube.Client.Network.Protocol
{
    public sealed class ClientBoundEnterWorldPacket
        : Packet<ClientBoundChunkLoadPacket, ClientPacketHandler>
    {
        public override void Encode(IByteBuffer buffer)
        {
            throw new NotImplementedException();
        }

        public override void Handle(ClientPacketHandler handler)
        {
            handler.game.OnWorldChange(guid, position.X, position.Y, position.Z);
        }

        public ClientBoundEnterWorldPacket(Guid guid, Vector3 position)
        {
            this.guid = guid;
            this.position = position;
        }

        public readonly Guid guid;

        public readonly Vector3 position;
    }
}
