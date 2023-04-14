using System;
using System.Runtime.InteropServices;

namespace StarCube.Utility.Guids
{
    public sealed class AutoIncrementGuidGenerator : IGuidGenerator
    {
        public ulong Current => current;

        public Guid Generate()
        {
            Span<ulong> buffer = stackalloc ulong[2];
            buffer[0] = current;
            buffer[1] = threadToken;
            current++;
            Span<byte> bytes = MemoryMarshal.AsBytes(buffer);
            return new Guid(bytes);
        }

        public AutoIncrementGuidGenerator(uint threadToken, ulong current)
        {
            this.threadToken = threadToken;
            this.current = current;
        }

        public readonly uint threadToken;
        private ulong current;
    }
}
