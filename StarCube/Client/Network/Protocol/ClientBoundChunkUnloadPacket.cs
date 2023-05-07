using System;

using StarCube.Utility.Math;
using StarCube.Network.Packets;
using StarCube.Network.ByteBuffer;

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
            if (handler.game.CurrentWorld != null && handler.game.CurrentWorld.TryGetClientLevel(guid, out var level))
            {
                level.ChunkRemoved(pos);
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
