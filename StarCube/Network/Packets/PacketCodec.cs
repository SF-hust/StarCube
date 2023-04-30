using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using StarCube.Network.ByteBuffer;
using StarCube.Network.ByteBuffer.Extensions;

namespace StarCube.Network.Packets
{
    public sealed class PacketCodec
    {
        public void Encode(Packet packet, IByteBuffer buffer)
        {
            buffer.WriteInt32(typeToIndex[packet.GetType()]);
            packet.Encode(buffer);
        }

        public P Decode<P>()
            where P : Packet
        {
            Packet packet = Decode<Packet>();
            if (packet is P p)
            {
                return p;
            }
            throw new InvalidCastException("cast packet failed");
        }

        public Packet Decode(IByteBuffer buffer)
        {
            int index = buffer.ReadInt32();
            var decode = indexToDecoder[index];
            return decode(buffer);
        }

        public PacketCodec(IEnumerable<Func<IByteBuffer, Packet>> decoders)
        {
            indexToDecoder = decoders.ToImmutableArray();
        }

        private readonly Dictionary<Type, int> typeToIndex = new Dictionary<Type, int>();

        private readonly ImmutableArray<Func<IByteBuffer, Packet>> indexToDecoder;
    }
}
