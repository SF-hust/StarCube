using System;

using StarCube.Network.ByteBuffer;
using StarCube.Network.Packets;
using StarCube.Utility.Math;

namespace StarCube.Client.Network.Protocol
{
    public sealed class ClientBoundChunkUnloadPacket
        : Packet<ClientBoundChunkLoadPacket, ClientPacketHandler>
    {
        public static ClientBoundChunkUnloadPacket Decode(IByteBuffer buffer)
        {
            throw new NotImplementedException();
        }

        public override void Encode(IByteBuffer buffer)
        {
            throw new NotImplementedException();
        }

        public override void Handle(ClientPacketHandler handler)
        {
            if (handler.game.World != null && handler.game.World.TryGetClientLevel(guid, out var level))
            {
                level.OnChunkUnload(pos);
            }
        }

        public ClientBoundChunkUnloadPacket(Guid guid, ChunkPos pos)
        {
            this.guid = guid;
            this.pos = pos;
        }

        public readonly Guid guid;

        public readonly ChunkPos pos;
    }
}
