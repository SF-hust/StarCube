using System;

namespace StarCube.Network.ByteBuffer
{
    public sealed class MemoryByteBufferReader : IByteBufferReader
    {
        public int Length => memory.Length - current;

        public byte ReadByte()
        {
            if (Length > 0)
            {
                byte b = memory.Span[current];
                current++;
                return b;
            }

            throw new IndexOutOfRangeException();
        }

        public void ReadBytes(Span<byte> bytes)
        {
            if (Length >= bytes.Length)
            {
                memory.Span.Slice(current, bytes.Length).CopyTo(bytes);
                current += bytes.Length;
                return;
            }

            throw new IndexOutOfRangeException();
        }

        public void Reset(Memory<byte> memory)
        {
            this.memory = memory;
            current = 0;
        }

        public MemoryByteBufferReader(Memory<byte> memory)
        {
            this.memory = memory;
            current = 0;
        }

        private Memory<byte> memory;

        private int current;
    }
}
