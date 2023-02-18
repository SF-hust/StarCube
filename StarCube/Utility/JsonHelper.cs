using System;
using System.Diagnostics.CodeAnalysis;

using Newtonsoft.Json.Linq;

namespace StarCube.Utility
{
    public static class JsonHelper
    {
        public static bool TryGetBoolean(this JObject json, string propertyName, out bool value)
        {
            value = default;
            return json.TryGetValue(propertyName, out JToken? token) && token.TryConvertToBoolean(out value);
        }

        public static bool TryGetInt32(this JObject json, string propertyName, out int value)
        {
            value = default;
            return json.TryGetValue(propertyName, out JToken? token) && token.TryConvertToInt32(out value);
        }

        public static bool TryGetUInt32(this JObject json, string propertyName, out uint value)
        {
            value = default;
            return json.TryGetValue(propertyName, out JToken? token) && token.TryConvertToUInt32(out value);
        }

        public static bool TryGetInt64(this JObject json, string propertyName, out long value)
        {
            value = default;
            return json.TryGetValue(propertyName, out JToken? token) && token.TryConvertToInt64(out value);
        }

        public static bool TryGetUInt64(this JObject json, string propertyName, out ulong value)
        {
            value = default;
            return json.TryGetValue(propertyName, out JToken? token) && token.TryConvertToUInt64(out value);
        }

        public static bool TryGetFloat(this JObject json, string propertyName, out float value)
        {
            value = default;
            return json.TryGetValue(propertyName, out JToken? token) && token.TryConvertToFloat(out value);
        }

        public static bool TryGetDouble(this JObject json, string propertyName, out double value)
        {
            value = default;
            return json.TryGetValue(propertyName, out JToken? token) && token.TryConvertToDouble(out value);
        }

        public static bool TryGetDouble(this JObject json, string propertyName, out Guid value)
        {
            value = default;
            return json.TryGetValue(propertyName, out JToken? token) && token.TryConvertToGuid(out value);
        }

        public static bool TryGetString(this JObject json, string propertyName, out string value)
        {
            value = string.Empty;
            return json.TryGetValue(propertyName, out JToken? token) && token.TryConvertToString(out value);
        }

        public static bool TryGetArray(this JObject json, string propertyName, [NotNullWhen(true)] out JArray? array)
        {
            if (json.TryGetValue(propertyName, out JToken? token) && token is JArray arr)
            {
                array = arr;
                return true;
            }
            array = null;
            return false;
        }

        public static bool TryGetJObject(this JObject json, string propertyName, [NotNullWhen(true)] out JObject? array)
        {
            if (json.TryGetValue(propertyName, out JToken? token) && token is JObject obj)
            {
                array = obj;
                return true;
            }
            array = null;
            return false;
        }

        public static bool TryConvertToBoolean(this JToken token, out bool value)
        {
            if (token is JValue jValue && jValue.Type == JTokenType.Boolean)
            {
                value = (bool)jValue;
                return true;
            }
            value = default;
            return false;
        }

        public static bool TryConvertToInt32(this JToken token, out int value)
        {
            if (token is JValue jValue && jValue.Type == JTokenType.Integer)
            {
                value = (int)jValue;
                return true;
            }
            value = default;
            return false;
        }

        public static bool TryConvertToUInt32(this JToken token, out uint value)
        {
            if (token is JValue jValue && jValue.Type == JTokenType.Integer)
            {
                value = (uint)jValue;
                return true;
            }
            value = default;
            return false;
        }

        public static bool TryConvertToInt64(this JToken token, out long value)
        {
            if (token is JValue jValue && jValue.Type == JTokenType.Integer)
            {
                value = (long)jValue;
                return true;
            }
            value = default;
            return false;
        }

        public static bool TryConvertToUInt64(this JToken token, out ulong value)
        {
            if (token is JValue jValue && jValue.Type == JTokenType.Integer)
            {
                value = (ulong)jValue;
                return true;
            }
            value = default;
            return false;
        }

        public static bool TryConvertToFloat(this JToken token, out float value)
        {
            if (token is JValue jValue && jValue.Type == JTokenType.Float)
            {
                value = (float)jValue;
                return true;
            }
            value = default;
            return false;
        }

        public static bool TryConvertToDouble(this JToken token, out double value)
        {
            if (token is JValue jValue && jValue.Type == JTokenType.Float)
            {
                value = (double)jValue;
                return true;
            }
            value = default;
            return false;
        }

        public static bool TryConvertToGuid(this JToken token, out Guid value)
        {
            if (token is JValue jValue && jValue.Type == JTokenType.Guid)
            {
                value = (Guid)jValue;
                return true;
            }
            value = default;
            return false;
        }

        public static bool TryConvertToString(this JToken token, out string value)
        {
            if(token is JValue jValue && jValue.Value is string v)
            {
                value = v;
                return true;
            }
            value = string.Empty;
            return false;
        }
    }
}
