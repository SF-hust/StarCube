using System;
using System.Runtime.InteropServices;
using System.Text;

using LiteDB;

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
            buffer.WriteInt64(value.ToBinary());
        }

        public static void WriteString(this IByteBuffer buffer, Span<char> value)
        {
            int length = Encoding.UTF8.GetByteCount(value);
            Span<byte> bytes = stackalloc byte[length];
            Encoding.UTF8.GetBytes(value, bytes);
            buffer.WriteInt32(length);
            buffer.WriteBytes(bytes);
        }

        public static void WriteBson(this IByteBuffer buffer, BsonDocument bson)
        {
            byte[] bytes = BsonSerializer.Serialize(bson);
            buffer.WriteInt32(bytes.Length);
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
            return new DateTime(buffer.ReadInt64());
        }

        public static string ReadString(this IByteBuffer buffer)
        {
            int length = buffer.ReadInt32();
            Span<byte> bytes = stackalloc byte[length];
            buffer.ReadBytes(bytes);
            return Encoding.UTF8.GetString(bytes);
        }

        public static BsonDocument ReadBson(this IByteBuffer buffer)
        {
            int length = buffer.ReadInt32();
            byte[] bytes = new byte[length];
            return BsonSerializer.Deserialize(bytes);
        }
    }
}
