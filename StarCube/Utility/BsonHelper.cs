using System;
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

        public static bool TryGetVector3F(this BsonDocument bson, string key, out Vector3 value)
        {
            if (bson.TryGetValue(key, out BsonValue? bValue) && bValue.IsBinary)
            {
                byte[] binary = bValue.AsBinary;
                if(binary.Length == 12)
                {
                    float x = BitConverter.ToSingle(binary, 0);
                    float y = BitConverter.ToSingle(binary, 4);
                    float z = BitConverter.ToSingle(binary, 8);
                    value = new Vector3(x, y, z);
                    return true;
                }
            }

            value = default;
            return false;
        }
    }
}
