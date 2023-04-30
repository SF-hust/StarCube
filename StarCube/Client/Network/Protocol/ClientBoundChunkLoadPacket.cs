using System;

using StarCube.Network.Packets;
using StarCube.Network.ByteBuffer;
using StarCube.Game.Levels.Chunks;

namespace StarCube.Client.Network.Protocol
{
    public sealed class ClientBoundChunkLoadPacket
        : Packet<ClientBoundChunkLoadPacket, ClientPacketHandler>
    {
        public static ClientBoundChunkLoadPacket Decode(IByteBuffer buffer)
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
                level.OnChunkLoad(chunk);
            }
        }

        public ClientBoundChunkLoadPacket(Guid guid, Chunk chunk)
        {
            this.guid = guid;
            this.chunk = chunk;
        }

        public readonly Guid guid;

        public readonly Chunk chunk;
    }
}
