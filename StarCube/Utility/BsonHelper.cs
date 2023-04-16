using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

using LiteDB;

namespace StarCube.Utility
{
    public static class BsonHelper
    {
        public static bool TryGetBoolean(this BsonDocument bson, string key, out bool value)
        {
            if (bson.TryGetValue(key, out BsonValue? bValue) && bValue.IsBoolean)
            {
                value = bValue.AsBoolean;
                return true;
            }

            value = default;
            return false;
        }

        public static bool TryGetInteger(this BsonDocument bson, string key, out int value)
        {
            if (TryGetInt32(bson, key, out value))
            {
                return true;
            }

            if (TryGetInt64(bson, key, out long longValue))
            {
                value = (int)longValue;
                return true;
            }

            return false;
        }

        public static bool TryGetInteger(this BsonDocument bson, string key, out long value)
        {
            if (TryGetInt64(bson, key, out value))
            {
                return true;
            }

            if (TryGetInt32(bson, key, out int intValue))
            {
                value = intValue;
                return true;
            }

            return false;
        }

        public static bool TryGetInt32(this BsonDocument bson, string key, out int value)
        {
            if (bson.TryGetValue(key, out BsonValue? bValue) && bValue.IsInt32)
            {
                value = bValue.AsInt32;
                return true;
            }

            value = default;
            return false;
        }

        public static bool TryGetInt64(this BsonDocument bson, string key, out long value)
        {
            if (bson.TryGetValue(key, out BsonValue? bValue) && bValue.IsInt64)
            {
                value = bValue.AsInt64;
                return true;
            }

            value = default;
            return false;
        }

        public static bool TryGetDouble(this BsonDocument bson, string key, out double value)
        {
            if (bson.TryGetValue(key, out BsonValue? bValue) && bValue.IsDouble)
            {
                value = bValue.AsDouble;
                return true;
            }

            value = default;
            return false;
        }

        public static bool TryGetDecimal(this BsonDocument bson, string key, out decimal value)
        {
            if (bson.TryGetValue(key, out BsonValue? bValue) && bValue.IsDecimal)
            {
                value = bValue.AsDecimal;
                return true;
            }

            value = default;
            return false;
        }

        public static bool TryGetGuid(this BsonDocument bson, string key, out Guid value)
        {
            if (bson.TryGetValue(key, out BsonValue? bValue) && bValue.IsGuid)
            {
                value = bValue.AsGuid;
                return true;
            }

            value = default;
            return false;
        }

        public static bool TryGetString(this BsonDocument bson, string key, out string value)
        {
            if (bson.TryGetValue(key, out BsonValue? bValue) && bValue.IsString)
            {
                value = bValue.AsString;
                return true;
            }

            value = string.Empty;
            return false;
        }

        public static bool TryGetDocument(this BsonDocument bson, string key, [NotNullWhen(true)] out BsonDocument? value)
        {
            if (bson.TryGetValue(key, out BsonValue? bValue) && bValue is BsonDocument bDoc)
            {
                value = bDoc;
                return true;
            }

            value = null;
            return false;
        }

        public static bool TryGetBinary(this BsonDocument bson, string key, out byte[] value)
        {
            if (bson.TryGetValue(key, out BsonValue? bValue) && bValue.IsBinary)
            {
                value = bValue.AsBinary;
                return true;
            }

            value = Array.Empty<byte>();
            return false;
        }

        public static bool TryGetStringID(this BsonDocument bson, string key, out StringID value)
        {
            if (bson.TryGetValue(key, out BsonValue? bValue) && bValue.IsString && StringID.TryParse(bValue.AsString, out value))
            {
                return true;
            }

            value = StringID.Failed;
            return false;
        }

        public static bool TryGetVector2(this BsonDocument bson, string key, out Vector2 value)
        {
            if (bson.TryGetValue(key, out BsonValue? bValue) && bValue.IsBinary)
            {
                byte[] binary = bValue.AsBinary;
                if (binary.Length == 8)
                {
                    if(BitConverter.IsLittleEndian)
                    {
                        binary.AsSpan(0, 4).Reverse();
                        binary.AsSpan(4, 4).Reverse();
                    }
                    float x = BitConverter.ToSingle(binary.AsSpan(0, 4));
                    float y = BitConverter.ToSingle(binary.AsSpan(4, 4));

                    value = new Vector2(x, y);
                    return true;
                }
            }

            value = default;
            return false;
        }

        public static bool TryGetVector3(this BsonDocument bson, string key, out Vector3 value)
        {
            if (bson.TryGetValue(key, out BsonValue? bValue) && bValue.IsBinary)
            {
                byte[] binary = bValue.AsBinary;
                if(binary.Length == 12)
                {
                    if (BitConverter.IsLittleEndian)
                    {
                        binary.AsSpan(0, 4).Reverse();
                        binary.AsSpan(4, 4).Reverse();
                        binary.AsSpan(8, 4).Reverse();
                    }
                    float x = BitConverter.ToSingle(binary.AsSpan(0, 4));
                    float y = BitConverter.ToSingle(binary.AsSpan(4, 4));
                    float z = BitConverter.ToSingle(binary.AsSpan(8, 4));

                    value = new Vector3(x, y, z);
                    return true;
                }
            }

            value = default;
            return false;
        }

        public static bool TryGetVector4(this BsonDocument bson, string key, out Vector4 value)
        {
            if (bson.TryGetValue(key, out BsonValue? bValue) && bValue.IsBinary)
            {
                byte[] binary = bValue.AsBinary;
                if (binary.Length == 16)
                {
                    if (BitConverter.IsLittleEndian)
                    {
                        binary.AsSpan(0, 4).Reverse();
                        binary.AsSpan(4, 4).Reverse();
                        binary.AsSpan(8, 4).Reverse();
                        binary.AsSpan(12, 4).Reverse();
                    }
                    float x = BitConverter.ToSingle(binary.AsSpan(0, 4));
                    float y = BitConverter.ToSingle(binary.AsSpan(4, 4));
                    float z = BitConverter.ToSingle(binary.AsSpan(8, 4));
                    float w = BitConverter.ToSingle(binary.AsSpan(12, 4));

                    value = new Vector4(x, y, z, w);
                    return true;
                }
            }

            value = default;
            return false;
        }

        public static bool TryGetQuaternion(this BsonDocument bson, string key, out Quaternion value)
        {
            if (bson.TryGetValue(key, out BsonValue? bValue) && bValue.IsBinary)
            {
                byte[] binary = bValue.AsBinary;
                if (binary.Length == 16)
                {
                    if (BitConverter.IsLittleEndian)
                    {
                        binary.AsSpan(0, 4).Reverse();
                        binary.AsSpan(4, 4).Reverse();
                        binary.AsSpan(8, 4).Reverse();
                        binary.AsSpan(12, 4).Reverse();
                    }
                    float x = BitConverter.ToSingle(binary.AsSpan(0, 4));
                    float y = BitConverter.ToSingle(binary.AsSpan(4, 4));
                    float z = BitConverter.ToSingle(binary.AsSpan(8, 4));
                    float w = BitConverter.ToSingle(binary.AsSpan(12, 4));

                    value = new Quaternion(x, y, z, w);
                    return true;
                }
            }

            value = default;
            return false;
        }

        public static bool TryGetMatrix4x4(this BsonDocument bson, string key, out Matrix4x4 value)
        {
            if (bson.TryGetValue(key, out BsonValue? bValue) && bValue.IsBinary)
            {
                byte[] binary = bValue.AsBinary;
                if (binary.Length == 64)
                {
                    if (BitConverter.IsLittleEndian)
                    {
                        for (int i = 0; i < 64; i += 4)
                        {
                            binary.AsSpan(i, 4).Reverse();
                        }
                    }
                    value = new Matrix4x4
                    {
                        M11 = BitConverter.ToSingle(binary, 0),
                        M12 = BitConverter.ToSingle(binary, 4),
                        M13 = BitConverter.ToSingle(binary, 8),
                        M14 = BitConverter.ToSingle(binary, 12),
                        M21 = BitConverter.ToSingle(binary, 16),
                        M22 = BitConverter.ToSingle(binary, 20),
                        M23 = BitConverter.ToSingle(binary, 24),
                        M24 = BitConverter.ToSingle(binary, 28),
                        M31 = BitConverter.ToSingle(binary, 32),
                        M32 = BitConverter.ToSingle(binary, 36),
                        M33 = BitConverter.ToSingle(binary, 40),
                        M34 = BitConverter.ToSingle(binary, 44),
                        M41 = BitConverter.ToSingle(binary, 48),
                        M42 = BitConverter.ToSingle(binary, 52),
                        M43 = BitConverter.ToSingle(binary, 56),
                        M44 = BitConverter.ToSingle(binary, 60)
                    };
                    return true;
                }
            }

            value = default;
            return false;
        }

        public static void Add(this BsonDocument bson, string key, StringID value)
        {
            bson.Add(key, new BsonValue(value.idString));
        }

        public static void Add(this BsonDocument bson, string key, Vector2 value)
        {
            byte[] binary = new byte[8];
            BitConverter.TryWriteBytes(binary.AsSpan(0, 4), value.X);
            BitConverter.TryWriteBytes(binary.AsSpan(4, 4), value.Y);
            if (BitConverter.IsLittleEndian)
            {
                binary.AsSpan(0, 4).Reverse();
                binary.AsSpan(4, 4).Reverse();
            }

            bson.Add(key, new BsonValue(binary));
        }

        public static void Add(this BsonDocument bson, string key, Vector3 value)
        {
            byte[] binary = new byte[12];
            BitConverter.TryWriteBytes(binary.AsSpan(0, 4), value.X);
            BitConverter.TryWriteBytes(binary.AsSpan(4, 4), value.Y);
            BitConverter.TryWriteBytes(binary.AsSpan(8, 4), value.Z);
            if (BitConverter.IsLittleEndian)
            {
                binary.AsSpan(0, 4).Reverse();
                binary.AsSpan(4, 4).Reverse();
                binary.AsSpan(8, 4).Reverse();
            }

            bson.Add(key, new BsonValue(binary));
        }

        public static void Add(this BsonDocument bson, string key, Vector4 value)
        {
            byte[] binary = new byte[16];
            BitConverter.TryWriteBytes(binary.AsSpan(0, 4), value.X);
            BitConverter.TryWriteBytes(binary.AsSpan(4, 4), value.Y);
            BitConverter.TryWriteBytes(binary.AsSpan(8, 4), value.Z);
            BitConverter.TryWriteBytes(binary.AsSpan(12, 4), value.W);
            if (BitConverter.IsLittleEndian)
            {
                binary.AsSpan(0, 4).Reverse();
                binary.AsSpan(4, 4).Reverse();
                binary.AsSpan(8, 4).Reverse();
                binary.AsSpan(12, 4).Reverse();
            }

            bson.Add(key, new BsonValue(binary));
        }

        public static void Add(this BsonDocument bson, string key, Quaternion value)
        {
            byte[] binary = new byte[16];
            BitConverter.TryWriteBytes(binary.AsSpan(0, 4), value.X);
            BitConverter.TryWriteBytes(binary.AsSpan(4, 4), value.Y);
            BitConverter.TryWriteBytes(binary.AsSpan(8, 4), value.Z);
            BitConverter.TryWriteBytes(binary.AsSpan(12, 4), value.W);
            if (BitConverter.IsLittleEndian)
            {
                binary.AsSpan(0, 4).Reverse();
                binary.AsSpan(4, 4).Reverse();
                binary.AsSpan(8, 4).Reverse();
                binary.AsSpan(12, 4).Reverse();
            }

            bson.Add(key, new BsonValue(binary));
        }

        public static void Add(this BsonDocument bson, string key, Matrix4x4 value)
        {
            byte[] binary = new byte[64];
            BitConverter.TryWriteBytes(binary.AsSpan(0, 4), value.M11);
            BitConverter.TryWriteBytes(binary.AsSpan(4, 4), value.M12);
            BitConverter.TryWriteBytes(binary.AsSpan(8, 4), value.M13);
            BitConverter.TryWriteBytes(binary.AsSpan(12, 4), value.M14);
            BitConverter.TryWriteBytes(binary.AsSpan(16, 4), value.M21);
            BitConverter.TryWriteBytes(binary.AsSpan(20, 4), value.M22);
            BitConverter.TryWriteBytes(binary.AsSpan(24, 4), value.M23);
            BitConverter.TryWriteBytes(binary.AsSpan(28, 4), value.M24);
            BitConverter.TryWriteBytes(binary.AsSpan(32, 4), value.M31);
            BitConverter.TryWriteBytes(binary.AsSpan(36, 4), value.M32);
            BitConverter.TryWriteBytes(binary.AsSpan(40, 4), value.M33);
            BitConverter.TryWriteBytes(binary.AsSpan(44, 4), value.M34);
            BitConverter.TryWriteBytes(binary.AsSpan(48, 4), value.M41);
            BitConverter.TryWriteBytes(binary.AsSpan(52, 4), value.M42);
            BitConverter.TryWriteBytes(binary.AsSpan(56, 4), value.M43);
            BitConverter.TryWriteBytes(binary.AsSpan(60, 4), value.M44);
            if (BitConverter.IsLittleEndian)
            {
                for (int i = 0; i < 64; i += 4)
                {
                    binary.AsSpan(i, 4).Reverse();
                }
            }

            bson.Add(key, new BsonValue(binary));
        }
    }
}
