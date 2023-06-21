using System;
using System.Runtime.InteropServices;
using System.Text;

using Newtonsoft.Json.Linq;

using LiteDB;

using StarCube.Utility.Math;

namespace StarCube.Network.ByteBuffer
{
    public interface IByteBufferWriter
    {
        public int Length { get; }

        /// <summary>
        /// 写入一个字节
        /// </summary>
        /// <param name="value"></param>
        public void WriteByte(byte value);

        /// <summary>
        /// 写入若干个字节
        /// </summary>
        /// <param name="bytes"></param>
        public void WriteBytes(ReadOnlySpan<byte> bytes);

        /// <summary>
        /// 将 value 以小端序写入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        private void WriteLittleEndian<T>(T value)
            where T : unmanaged
        {
            Span<T> valueSpan = stackalloc T[1] { value };
            Span<byte> bytes = MemoryMarshal.AsBytes(valueSpan);
            if (!BitConverter.IsLittleEndian)
            {
                bytes.Reverse();
            }
            WriteBytes(bytes);
        }

        /// <summary>
        /// 写入 16 位有符号整数
        /// </summary>
        /// <param name="value"></param>
        public void WriteInt16(short value)
        {
            WriteLittleEndian(value);
        }

        /// <summary>
        /// 写入 16 位无符号整数
        /// </summary>
        /// <param name="value"></param>
        public void WriteUInt16(ushort value)
        {
            WriteLittleEndian(value);
        }

        /// <summary>
        /// 写入 32 位有符号整数
        /// </summary>
        /// <param name="value"></param>
        public void WriteInt32(int value)
        {
            WriteLittleEndian(value);
        }

        /// <summary>
        /// 写入 32 位无符号整数
        /// </summary>
        /// <param name="value"></param>
        public void WriteUInt32(uint value)
        {
            WriteLittleEndian(value);
        }

        /// <summary>
        /// 写入 64 位有符号整数
        /// </summary>
        /// <param name="value"></param>
        public void WriteInt64(long value)
        {
            WriteLittleEndian(value);
        }

        /// <summary>
        /// 写入 64 位无符号整数
        /// </summary>
        /// <param name="value"></param>
        public void WriteUInt64(ulong value)
        {
            WriteLittleEndian(value);
        }

        /// <summary>
        /// 以可变长方式写入 32 位有符号整数
        /// </summary>
        /// <param name="value"></param>
        public void WriteVarInt(int value)
        {
            WriteVarUInt(BitUtil.ZigZagEncode(value));
        }

        /// <summary>
        /// 以可变长方式写入 32 位无符号整数
        /// </summary>
        /// <param name="value"></param>
        public void WriteVarUInt(uint value)
        {
            Span<byte> bytes = stackalloc byte[5];
            int length = 1;
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)((value >> (i * 7)) & 0x7FU | 0x80U);
                if (bytes[i] != 0x80)
                {
                    length = i + 1;
                }
            }
            bytes[length - 1] &= 0x7F;
            WriteBytes(bytes[..length]);
        }

        /// <summary>
        /// 以可变长方式写入 64 位有符号整数
        /// </summary>
        /// <param name="value"></param>
        public void WriteVarInt(long value)
        {
            WriteVarUInt(BitUtil.ZigZagEncode(value));
        }

        /// <summary>
        /// 以可变长方式写入 64 位无符号整数
        /// </summary>
        /// <param name="value"></param>
        public void WriteVarUInt(ulong value)
        {
            Span<byte> bytes = stackalloc byte[10];
            int length = 1;
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)((value >> (i * 7)) & 0x7FU | 0x80U);
                if (bytes[i] != 0x80)
                {
                    length = i + 1;
                }
            }
            bytes[length - 1] &= 0x7F;
            WriteBytes(bytes[..length]);
        }

        /// <summary>
        /// 写入 32 位浮点数
        /// </summary>
        /// <param name="value"></param>
        public void WriteSingle(float value)
        {
            WriteLittleEndian(value);
        }

        /// <summary>
        /// 写入 64 位浮点数
        /// </summary>
        /// <param name="value"></param>
        public void WriteDouble(double value)
        {
            WriteLittleEndian(value);
        }

        /// <summary>
        /// 写入布尔值
        /// </summary>
        /// <param name="value"></param>
        public void WriteBool(bool value)
        {
            WriteByte(value ? (byte)1 : (byte)0);
        }

        /// <summary>
        /// 写入十进制小数
        /// </summary>
        /// <param name="value"></param>
        public void WriteDecimal(decimal value)
        {
            WriteLittleEndian(value);
        }

        /// <summary>
        /// 写入 Guid
        /// </summary>
        /// <param name="value"></param>
        public void WriteGuid(Guid value)
        {
            Span<byte> bytes = stackalloc byte[16];
            value.TryWriteBytes(bytes);
            WriteBytes(bytes);
        }

        /// <summary>
        /// 写入本地时间
        /// </summary>
        /// <param name="value"></param>
        public void WriteDateTime(DateTime value)
        {
            WriteInt64(value.ToBinary());
        }

        /// <summary>
        /// 写入 UTC 时间
        /// </summary>
        /// <param name="value"></param>
        public void WriteDateTimeUTC(DateTime value)
        {
            WriteInt64(value.ToUniversalTime().Ticks);
        }

        /// <summary>
        /// 写入字符串
        /// </summary>
        /// <param name="value"></param>
        public void WriteString(ReadOnlySpan<char> value)
        {
            int length = Encoding.UTF8.GetByteCount(value);
            Span<byte> bytes = stackalloc byte[length];
            Encoding.UTF8.GetBytes(value, bytes);
            WriteVarUInt(unchecked((uint)length));
            WriteBytes(bytes);
        }

        /// <summary>
        /// 写入 Json 文档
        /// </summary>
        /// <param name="value"></param>
        public void WriteJson(JObject value)
        {
            WriteString(value.ToString());
        }

        /// <summary>
        /// 写入 Bson 文档
        /// </summary>
        /// <param name="value"></param>
        public void WriteBson(BsonDocument value)
        {
            byte[] bytes = BsonSerializer.Serialize(value);
            WriteVarUInt((uint)bytes.Length);
            WriteBytes(bytes);
        }
    }
}
