using System;
using System.Runtime.InteropServices;
using System.Text;

using Newtonsoft.Json.Linq;

using LiteDB;

using StarCube.Utility.Math;

namespace StarCube.Network.ByteBuffer
{
    internal interface IByteBufferReader
    {
        public byte ReadByte();

        public void ReadBytes(Span<byte> bytes);

        private T Read<T>()
            where T : unmanaged
        {
            Span<T> valueSpan = stackalloc T[1];
            Span<byte> bytes = MemoryMarshal.AsBytes(valueSpan);
            ReadBytes(bytes);
            if (BitConverter.IsLittleEndian)
            {
                bytes.Reverse();
            }
            return valueSpan[0];
        }

        public short ReadInt16()
        {
            return Read<short>();
        }

        public ushort ReadUInt16()
        {
            return Read<ushort>();
        }

        public int ReadInt32()
        {
            return Read<int>();
        }

        public uint ReadUInt32()
        {
            return Read<uint>();
        }

        public long ReadInt64()
        {
            return Read<long>();
        }

        public ulong ReadUInt64()
        {
            return Read<ulong>();
        }

        public int ReadVarInt32()
        {
            uint value = ReadVarUInt32();
            return BitUtil.ZigZagDecode(value);
        }

        public uint ReadVarUInt32()
        {
            byte b;
            int count = 0;
            uint value = 0U;
            do
            {
                b = ReadByte();
                value |= unchecked((uint)(b & 0x7F) << (7 * count));
                count++;
            }
            while ((b & 0x80) != 0 && count < 5);
            return value;
        }

        public long ReadVarInt64()
        {
            ulong value = ReadVarUInt64();
            return BitUtil.ZigZagDecode(value);
        }

        public ulong ReadVarUInt64()
        {
            byte b;
            int count = 0;
            ulong value = 0LU;
            do
            {
                b = ReadByte();
                value |= unchecked((ulong)(b & 0x7F) << (7 * count));
                count++;
            }
            while ((b & 0x80) != 0 && count < 10);
            return value;
        }

        public float ReadSingle()
        {
            return Read<float>();
        }

        public double ReadDouble()
        {
            return Read<double>();
        }

        public bool ReadBool()
        {
            byte b = ReadByte();
            return b != 0;
        }

        public decimal ReadDecimal()
        {
            return Read<decimal>();
        }

        public Guid ReadGuid()
        {
            Span<byte> bytes = stackalloc byte[16];
            ReadBytes(bytes);
            return new Guid(bytes);
        }

        public DateTime ReadDateTime()
        {
            return DateTime.FromBinary(ReadInt64());
        }

        public DateTime ReadDateTimeUTC()
        {
            return new DateTime(ReadInt64(), DateTimeKind.Utc);
        }

        public string ReadString()
        {
            int length = unchecked((int)ReadVarUInt32());
            Span<byte> bytes = stackalloc byte[length];
            ReadBytes(bytes);
            return Encoding.UTF8.GetString(bytes);
        }

        public JObject ReadJson()
        {
            return JObject.Parse(ReadString());
        }

        public BsonDocument ReadBson()
        {
            int length = (int)ReadVarUInt32();
            byte[] bytes = new byte[length];
            ReadBytes(bytes);
            return BsonSerializer.Deserialize(bytes);
        }
    }
}
