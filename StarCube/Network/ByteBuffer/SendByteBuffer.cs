using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace StarCube.Network.ByteBuffer
{
    public sealed class SendByteBuffer : IByteBuffer
    {
        public int Length => length;

        /// <summary>
        /// 从 ByteBuffer 中读取一个字节，不支持
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public byte ReadByte()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 从 ByteBuffer 中读取一个字节序列，不支持
        /// </summary>
        /// <param name="bytes"></param>
        /// <exception cref="NotSupportedException"></exception>
        public void ReadBytes(Span<byte> bytes)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 向 ByteBuffer 中同步写入一个字节
        /// </summary>
        /// <param name="value"></param>
        public void WriteByte(byte value)
        {
            if (buffer.Length == current)
            {
                Flush(1);
            }

            buffer.Span[current] = value;
            current++;
            length++;
        }

        /// <summary>
        /// 向 ByteBuffer 中同步写入一个字节序列
        /// </summary>
        /// <param name="bytes"></param>
        public void WriteBytes(ReadOnlySpan<byte> bytes)
        {
            if (buffer.Length - current < bytes.Length)
            {
                Flush(bytes.Length);
            }

            bytes.CopyTo(buffer.Span.Slice(current));
            current += bytes.Length;
            length += bytes.Length;
        }

        /// <summary>
        /// 将 ByteBuffer 中的内容同步写入 span 中，并清空 ByteBuffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <exception cref="ArgumentException"></exception>
        public void MoveTo(Span<byte> buffer)
        {
            if (buffer.Length < length)
            {
                throw new ArgumentException($"buffer is too small (require {length}, but only {buffer.Length})");
            }

            Flush();
            length = 0;
            if(pipe.Reader.TryRead(out var result))
            {
                result.Buffer.CopyTo(buffer);
                pipe.Reader.AdvanceTo(result.Buffer.End);
            }
        }

        /// <summary>
        /// 将 ByteBuffer 中的内容通过 socket 同步发送，并清空 ByteBuffer
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        public int Send(Socket socket)
        {
            Flush();
            length = 0;

            if (pipe.Reader.TryRead(out var result))
            {
                int count = 0;
                foreach (ReadOnlyMemory<byte> buffer in result.Buffer)
                {
                    count += socket.Send(buffer.Span, SocketFlags.None);
                }
                pipe.Reader.AdvanceTo(result.Buffer.End);
                return count;
            }

            return 0;
        }

        /// <summary>
        /// 将 ByteBuffer 中的内容通过 socket 异步发送，并清空 ByteBuffer
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        public async ValueTask<int> SendAsync(Socket socket)
        {
            Flush();
            length = 0;

            if (pipe.Reader.TryRead(out var result))
            {
                int count = 0;
                foreach (ReadOnlyMemory<byte> buffer in result.Buffer)
                {
                     count += await socket.SendAsync(buffer, SocketFlags.None);
                }
                pipe.Reader.AdvanceTo(result.Buffer.End);
                return count;
            }

            return 0;
        }

        /// <summary>
        /// 将当前 buffer 中的内容刷新到 pipe 中，并获取一个长度至少为 length 的新 buffer
        /// </summary>
        /// <param name="length"></param>
        private void Flush(int length = 0)
        {
            pipe.Writer.Advance(current);
            var task = pipe.Writer.FlushAsync();
            if (!task.IsCompleted)
            {
                task.AsTask().Wait();
            }
            buffer = pipe.Writer.GetMemory(length);
            current = 0;
        }

        public SendByteBuffer()
        {
            buffer = pipe.Writer.GetMemory();
        }

        private readonly Pipe pipe = new Pipe();

        private Memory<byte> buffer;

        private int current = 0;

        private int length = 0;
    }
}
