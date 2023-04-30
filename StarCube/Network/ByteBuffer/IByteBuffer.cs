using System;

namespace StarCube.Network.ByteBuffer
{
    public interface IByteBuffer
    {
        public void WriteByte(byte value);

        public void WriteBytes(ReadOnlySpan<byte> bytes);

        public byte ReadByte();

        public void ReadBytes(Span<byte> bytes);
    }
}
