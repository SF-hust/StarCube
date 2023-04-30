using System;

using StarCube.Network.ByteBuffer;

namespace StarCube.Network.Packets
{
    public abstract class Packet
    {
        public abstract void Encode(IByteBuffer buffer);

        public abstract void Handle(PacketHandler handler);
    }

    public abstract class Packet<P, H> : Packet
        where P : Packet<P, H>
        where H : PacketHandler
    {

        public sealed override void Handle(PacketHandler handler)
        {
            if (handler is H h)
            {
                Handle(h);
            }
            throw new InvalidCastException("handler type invalid");
        }

        public abstract void Handle(H handler);
    }
}
