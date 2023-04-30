using System;
using System.Buffers;

using System.IO.Pipelines;
using System.Net.Sockets;

namespace StarCube.Network.ByteBuffer
{
    public sealed class SimpleByteBuffer : IByteBuffer
    {
        public int Length => length;

        public byte ReadByte()
        {
            if (!pipe.Reader.TryRead(out var result) || result.Buffer.Length < 1)
            {
                throw new InvalidOperationException("no more data in byte buffer");
            }
            byte ret = result.Buffer.FirstSpan[0];
            pipe.Reader.AdvanceTo(result.Buffer.GetPosition(1));
            length--;
            return ret;
        }

        public void ReadBytes(Span<byte> bytes)
        {
            if (!pipe.Reader.TryRead(out var result) || result.Buffer.Length < bytes.Length)
            {
                throw new InvalidOperationException($"no more data in byte buffer (require {bytes.Length} bytes but {result.Buffer.Length} bytes rest)");
            }

            result.Buffer.CopyTo(bytes);
            pipe.Reader.AdvanceTo(result.Buffer.GetPosition(bytes.Length));
            length -= bytes.Length;
        }

        public void WriteByte(byte value)
        {
            Span<byte> buffer = pipe.Writer.GetSpan(1);
            buffer[0] = value;
            pipe.Writer.Advance(1);
            length++;
        }

        public void WriteBytes(ReadOnlySpan<byte> bytes)
        {
            Span<byte> buffer = pipe.Writer.GetSpan(bytes.Length);
            bytes.CopyTo(buffer);
            pipe.Writer.Advance(bytes.Length);
            length += bytes.Length;
        }

        public void SendAndClear(Socket socket)
        {
            pipe.Writer.Complete();
            if (!pipe.Reader.TryRead(out ReadResult result))
            {
                throw new InvalidOperationException($"no more data in byte buffer to send");
            }
            Clear();
        }

        public void Receive(Socket socket)
        {
        }

        public void Clear()
        {
            length = 0;
            pipe.Writer.Complete();
            pipe.Reader.Complete();
            pipe.Reset();
        }

        public SimpleByteBuffer()
        {
        }

        private readonly Pipe pipe = new Pipe();

        private int length = 0;
    }
}
