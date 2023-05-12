using System;
using System.Runtime.InteropServices;
using System.Text;

using LiteDB;

using StarCube.Utility.Math;

namespace StarCube.Network.ByteBuffer.Extensions
{
    public static class ByteBufferExtensions
    {
        private static void Write<T>(IByteBuffer buffer, T value)
            where T : unmanaged
        {
            Span<T> valueSpan = stackalloc T[1] { value };
            Span<byte> bytes = MemoryMarshal.AsBytes(valueSpan);
            if (BitConverter.IsLittleEndian)
            {
                bytes.Reverse();
            }
            buffer.WriteBytes(bytes);
        }

        public static void WriteInt16(this IByteBuffer buffer, short value)
        {
            Write(buffer, value);
        }

        public static void WriteUInt16(this IByteBuffer buffer, ushort value)
        {
            Write(buffer, value);
        }

        public static void WriteInt32(this IByteBuffer buffer, int value)
        {
            Write(buffer, value);
        }

        public static void WriteUInt32(this IByteBuffer buffer, uint value)
        {
            Write(buffer, value);
        }

        public static void WriteInt64(this IByteBuffer buffer, long value)
        {
            Write(buffer, value);
        }

        public static void WriteUInt64(this IByteBuffer buffer, ulong value)
        {
            Write(buffer, value);
        }

        public static void WriteVarInt(this IByteBuffer buffer, int value)
        {
            WriteVarUInt(buffer, BitUtil.ZigZagEncode(value));
        }

        public static void WriteVarUInt(this IByteBuffer buffer, uint value)
        {
            Span<byte> bytes = stackalloc byte[5];
            int count = 1;
            bytes[0] = (byte)(value & 0x7FU | 0x80U);
            bytes[1] = (byte)((value >> 7) & 0x7FU | 0x80U);
            bytes[2] = (byte)((value >> 14) & 0x7FU | 0x80U);
            bytes[3] = (byte)((value >> 21) & 0x7FU | 0x80U);
            bytes[4] = (byte)(value >> 28);
            if (bytes[1] != 0x80)
            {
                count = 2;
            }
            if (bytes[2] != 0x80)
            {
                count = 3;
            }
            if (bytes[3] != 0x80)
            {
                count = 4;
            }
            if (bytes[4] != 0)
            {
                count = 5;
            }
            bytes[count - 1] &= 0x7F;
            buffer.WriteBytes(bytes[..count]);
        }

        public static void WriteVarInt(this IByteBuffer buffer, long value)
        {
            WriteVarUInt(buffer, BitUtil.ZigZagEncode(value));
        }

        public static void WriteVarUInt(this IByteBuffer buffer, ulong value)
        {
            Span<byte> bytes = stackalloc byte[10];
            int count = 1;
            bytes[0] = (byte)(value & 0x7FU | 0x80U);
            bytes[1] = (byte)((value >> 7) & 0x7FU | 0x80U);
            bytes[2] = (byte)((value >> 14) & 0x7FU | 0x80U);
            bytes[3] = (byte)((value >> 21) & 0x7FU | 0x80U);
            bytes[4] = (byte)((value >> 28) & 0x7FU | 0x80U);
            bytes[5] = (byte)((value >> 35) & 0x7FU | 0x80U);
            bytes[6] = (byte)((value >> 42) & 0x7FU | 0x80U);
            bytes[7] = (byte)((value >> 49) & 0x7FU | 0x80U);
            bytes[8] = (byte)((value >> 56) & 0x7FU | 0x80U);
            bytes[9] = (byte)(value >> 63);
            if (bytes[1] != 0x80)
            {
                count = 2;
            }
            if (bytes[2] != 0x80)
            {
                count = 3;
            }
            if (bytes[3] != 0x80)
            {
                count = 4;
            }
            if (bytes[4] != 0x80)
            {
                count = 5;
            }
            if (bytes[5] != 0x80)
            {
                count = 6;
            }
            if (bytes[6] != 0x80)
            {
                count = 7;
            }
            if (bytes[7] != 0x80)
            {
                count = 8;
            }
            if (bytes[8] != 0x80)
            {
                count = 9;
            }
            if (bytes[9] != 0)
            {
                count = 10;
            }
            bytes[count - 1] &= 0x7F;
            buffer.WriteBytes(bytes[..count]);
        }

        public static void WriteSingle(this IByteBuffer buffer, float value)
        {
            Write(buffer, value);
        }

        public static void WriteDouble(this IByteBuffer buffer, double value)
        {
            Write(buffer, value);
        }

        public static void WriteBool(this IByteBuffer buffer, bool value)
        {
            buffer.WriteByte(value ? (byte)1 : (byte)0);
        }

        public static void WriteDecimal(this IByteBuffer buffer, decimal value)
        {
            Write(buffer, value);
        }

        public static void WriteGuid(this IByteBuffer buffer, Guid value)
        {
            Span<byte> bytes = stackalloc byte[16];
            value.TryWriteBytes(bytes);
            buffer.WriteBytes(bytes);
        }

        public static void WriteDateTime(this IByteBuffer buffer, DateTime value)
        {
            WriteInt64(buffer, value.ToBinary());
        }

        public static void WriteDateTimeUTC(this IByteBuffer buffer, DateTime value)
        {
            WriteInt64(buffer, value.ToUniversalTime().Ticks);
        }

        public static void WriteString(this IByteBuffer buffer, Span<char> value)
        {
            int length = Encoding.UTF8.GetByteCount(value);
            Span<byte> bytes = stackalloc byte[length];
            Encoding.UTF8.GetBytes(value, bytes);
            WriteVarUInt(buffer, unchecked((uint)length));
            buffer.WriteBytes(bytes);
        }

        public static void WriteBson(this IByteBuffer buffer, BsonDocument bson)
        {
            byte[] bytes = BsonSerializer.Serialize(bson);
            WriteVarUInt(buffer, (uint)bytes.Length);
            buffer.WriteBytes(bytes);
        }


        private static T Read<T>(IByteBuffer buffer)
            where T : unmanaged
        {
            Span<T> valueSpan = stackalloc T[1];
            Span<byte> bytes = MemoryMarshal.AsBytes(valueSpan);
            buffer.ReadBytes(bytes);
            if (BitConverter.IsLittleEndian)
            {
                bytes.Reverse();
            }
            return valueSpan[0];
        }

        public static short ReadInt16(this IByteBuffer buffer)
        {
            return Read<short>(buffer);
        }

        public static ushort ReadUInt16(this IByteBuffer buffer)
        {
            return Read<ushort>(buffer);
        }

        public static int ReadInt32(this IByteBuffer buffer)
        {
            return Read<int>(buffer);
        }

        public static uint ReadUInt32(this IByteBuffer buffer)
        {
            return Read<uint>(buffer);
        }

        public static long ReadInt64(this IByteBuffer buffer)
        {
            return Read<long>(buffer);
        }

        public static ulong ReadUInt64(this IByteBuffer buffer)
        {
            return Read<ulong>(buffer);
        }

        public static int ReadVarInt32(this IByteBuffer buffer)
        {
            uint value = ReadVarUInt32(buffer);
            return BitUtil.ZigZagDecode(value);
        }

        public static uint ReadVarUInt32(this IByteBuffer buffer)
        {
            byte b;
            int count = 0;
            uint value = 0U;
            do
            {
                b = buffer.ReadByte();
                value |= unchecked((uint)(b & 0x7F) << (7 * count));
                count++;
            }
            while ((b & 0x80) != 0 && count < 5);
            return value;
        }

        public static long ReadVarInt64(this IByteBuffer buffer)
        {
            ulong value = ReadVarUInt64(buffer);
            return BitUtil.ZigZagDecode(value);
        }

        public static ulong ReadVarUInt64(this IByteBuffer buffer)
        {
            byte b;
            int count = 0;
            ulong value = 0LU;
            do
            {
                b = buffer.ReadByte();
                value |= unchecked((ulong)(b & 0x7F) << (7 * count));
                count++;
            }
            while ((b & 0x80) != 0 && count < 10);
            return value;
        }

        public static float ReadSingle(this IByteBuffer buffer)
        {
            return Read<float>(buffer);
        }

        public static double ReadDouble(this IByteBuffer buffer)
        {
            return Read<double>(buffer);
        }

        public static bool ReadBool(this IByteBuffer buffer)
        {
            byte b = buffer.ReadByte();
            return b != 0;
        }

        public static decimal ReadDecimal(this IByteBuffer buffer)
        {
            return Read<decimal>(buffer);
        }

        public static Guid ReadGuid(this IByteBuffer buffer)
        {
            Span<byte> bytes = stackalloc byte[16];
            buffer.ReadBytes(bytes);
            return new Guid(bytes);
        }

        public static DateTime ReadDateTime(this IByteBuffer buffer)
        {
            return DateTime.FromBinary(ReadInt64(buffer));
        }

        public static DateTime ReadDateTimeUTC(this IByteBuffer buffer)
        {
            return new DateTime(ReadInt64(buffer), DateTimeKind.Utc);
        }

        public static string ReadString(this IByteBuffer buffer)
        {
            int length = unchecked((int)ReadVarUInt32(buffer));
            Span<byte> bytes = stackalloc byte[length];
            buffer.ReadBytes(bytes);
            return Encoding.UTF8.GetString(bytes);
        }

        public static BsonDocument ReadBson(this IByteBuffer buffer)
        {
            int length = (int)ReadVarUInt32(buffer);
            byte[] bytes = new byte[length];
            buffer.ReadBytes(bytes);
            return BsonSerializer.Deserialize(bytes);
        }
    }
}
