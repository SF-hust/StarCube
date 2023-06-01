using System;
using System.Buffers;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO.Pipelines;

namespace StarCube.Network.ByteBuffer
{
    public sealed class ReceiveByteBuffer : IByteBuffer
    {
        public bool IsEmpty => Length == 0;

        public int Length => (int)buffer.Length - current;

        public byte ReadByte()
        {
            if (!IsEmpty)
            {
                byte result = buffer.Slice(current).FirstSpan[0];
                current++;
                return result;
            }

            throw new InvalidOperationException("no more byte");
        }

        public void ReadBytes(Span<byte> bytes)
        {
            if (Length >= bytes.Length)
            {
                buffer.Slice(current).CopyTo(bytes);
                current += bytes.Length;
                return;
            }

            throw new InvalidOperationException($"no enough bytes, require ({bytes.Length}) but rest ({buffer.Length - current})");
        }

        /// <summary>
        /// 重新获取 pipe 中的数据
        /// </summary>
        private void ResetBuffer()
        {
            if(pipe.Reader.TryRead(out ReadResult result))
            {
                buffer = result.Buffer;
            }
            else
            {
                buffer = ReadOnlySequence<byte>.Empty;
            }
            current = 0;
        }

        public void WriteByte(byte value)
        {
            throw new NotSupportedException();
        }

        public void WriteBytes(ReadOnlySpan<byte> bytes)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 将 buffer 内容同步填充到 ByteBuffer 末尾
        /// </summary>
        /// <param name="buffer"></param>
        public void Append(ReadOnlySpan<byte> buffer)
        {
            Memory<byte> memory = pipe.Writer.GetMemory(buffer.Length);
            buffer.CopyTo(memory.Span);
            pipe.Writer.Advance(buffer.Length);

            Flush();
        }

        public void Flush()
        {
            var task = pipe.Writer.FlushAsync();
            if (!task.IsCompleted)
            {
                task.AsTask().Wait();
            }
            ResetBuffer();
        }

        /// <summary>
        /// 从 socket 中同步接受数据并填充至 ByteBuffer 末尾
        /// </summary>
        /// <param name="socket"></param>
        public void Receive(Socket socket)
        {
            int length;
            do
            {
                Memory<byte> buffer = pipe.Writer.GetMemory();
                length = socket.Receive(buffer.Span, SocketFlags.Peek);
                pipe.Writer.Advance(length);
            }
            while (length != 0);

            ResetBuffer();
        }

        /// <summary>
        /// 从 socket 中异步接受数据并填充至 ByteBuffer 末尾
        /// </summary>
        /// <param name="socket"></param>
        public async ValueTask ReceiveAsync(Socket socket)
        {
            int length;
            do
            {
                Memory<byte> buffer = pipe.Writer.GetMemory();
                length = await socket.ReceiveAsync(buffer, SocketFlags.Peek);
                pipe.Writer.Advance(length);
            }
            while (length != 0);

            ResetBuffer();
        }

        public ReceiveByteBuffer()
        {
        }

        private readonly Pipe pipe = new Pipe();

        private ReadOnlySequence<byte> buffer = ReadOnlySequence<byte>.Empty;

        private int current = 0;
    }
}
