using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StarCube.Utility
{
    public static class JsonHelper
    {
        public static bool TryGetBoolean(this JObject json, string propertyName, out bool value)
        {
            value = default;
            return json.TryGetValue(propertyName, out JToken? token) && token.ToBooleanValue(out value);
        }

        public static bool TryGetInt32(this JObject json, string propertyName, out int value)
        {
            value = default;
            return json.TryGetValue(propertyName, out JToken? token) && token.ToInt32Value(out value);
        }

        public static bool TryGetUInt32(this JObject json, string propertyName, out uint value)
        {
            value = default;
            return json.TryGetValue(propertyName, out JToken? token) && token.ToUInt32Value(out value);
        }

        public static bool TryGetInt64(this JObject json, string propertyName, out long value)
        {
            value = default;
            return json.TryGetValue(propertyName, out JToken? token) && token.ToInt64Value(out value);
        }

        public static bool TryGetUInt64(this JObject json, string propertyName, out ulong value)
        {
            value = default;
            return json.TryGetValue(propertyName, out JToken? token) && token.ToUInt64Value(out value);
        }

        public static bool TryGetFloat(this JObject json, string propertyName, out float value)
        {
            value = default;
            return json.TryGetValue(propertyName, out JToken? token) && token.ToFloatValue(out value);
        }

        public static bool TryGetDouble(this JObject json, string propertyName, out double value)
        {
            value = default;
            return json.TryGetValue(propertyName, out JToken? token) && token.ToDoubleValue(out value);
        }

        public static bool TryGetDouble(this JObject json, string propertyName, out Guid value)
        {
            value = default;
            return json.TryGetValue(propertyName, out JToken? token) && token.ToGuidValue(out value);
        }

        public static bool TryGetString(this JObject json, string propertyName, out string value)
        {
            value = string.Empty;
            return json.TryGetValue(propertyName, out JToken? token) && token.ToStringValue(out value);
        }

        public static bool TryGetStringID(this JObject json, string propertyName, out StringID value)
        {
            value = StringID.Failed;
            return json.TryGetValue(propertyName, out JToken? token) &&
                token.ToStringValue(out string idString) &&
                StringID.TryParse(idString, out value);
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

        public static bool TryGetJObject(this JObject json, string propertyName, [NotNullWhen(true)] out JObject? jObject)
        {
            if (json.TryGetValue(propertyName, out JToken? token) && token is JObject obj)
            {
                jObject = obj;
                return true;
            }
            jObject = null;
            return false;
        }

        public static bool ToBooleanValue(this JToken token, out bool value)
        {
            if (token is JValue jValue && jValue.Type == JTokenType.Boolean)
            {
                value = (bool)jValue;
                return true;
            }
            value = default;
            return false;
        }

        public static bool ToInt32Value(this JToken token, out int value)
        {
            if (token is JValue jValue && jValue.Type == JTokenType.Integer)
            {
                value = (int)jValue;
                return true;
            }
            value = default;
            return false;
        }

        public static bool ToUInt32Value(this JToken token, out uint value)
        {
            if (token is JValue jValue && jValue.Type == JTokenType.Integer)
            {
                value = (uint)jValue;
                return true;
            }
            value = default;
            return false;
        }

        public static bool ToInt64Value(this JToken token, out long value)
        {
            if (token is JValue jValue && jValue.Type == JTokenType.Integer)
            {
                value = (long)jValue;
                return true;
            }
            value = default;
            return false;
        }

        public static bool ToUInt64Value(this JToken token, out ulong value)
        {
            if (token is JValue jValue && jValue.Type == JTokenType.Integer)
            {
                value = (ulong)jValue;
                return true;
            }
            value = default;
            return false;
        }

        public static bool ToFloatValue(this JToken token, out float value)
        {
            if (token is JValue jValue && jValue.Type == JTokenType.Float)
            {
                value = (float)jValue;
                return true;
            }
            value = default;
            return false;
        }

        public static bool ToDoubleValue(this JToken token, out double value)
        {
            if (token is JValue jValue && jValue.Type == JTokenType.Float)
            {
                value = (double)jValue;
                return true;
            }
            value = default;
            return false;
        }

        public static bool ToGuidValue(this JToken token, out Guid value)
        {
            if (token is JValue jValue && jValue.Type == JTokenType.Guid)
            {
                value = (Guid)jValue;
                return true;
            }
            value = default;
            return false;
        }

        public static bool ToStringValue(this JToken token, out string value)
        {
            if(token is JValue jValue && jValue.Value is string v)
            {
                value = v;
                return true;
            }
            value = string.Empty;
            return false;
        }

        public static bool ToBooleanArray(this JArray jArray, out bool[] array)
        {
            array = new bool[jArray.Count];
            for(int i = 0; i < jArray.Count; i++)
            {
                if(!ToBooleanValue(jArray[i], out array[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool ToInt32Array(this JArray jArray, out int[] array)
        {
            array = new int[jArray.Count];
            for (int i = 0; i < jArray.Count; i++)
            {
                if (!ToInt32Value(jArray[i], out array[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool ToUInt32Array(this JArray jArray, out uint[] array)
        {
            array = new uint[jArray.Count];
            for (int i = 0; i < jArray.Count; i++)
            {
                if (!ToUInt32Value(jArray[i], out array[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool ToInt64Array(this JArray jArray, out long[] array)
        {
            array = new long[jArray.Count];
            for (int i = 0; i < jArray.Count; i++)
            {
                if (!ToInt64Value(jArray[i], out array[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool ToUInt64Array(this JArray jArray, out ulong[] array)
        {
            array = new ulong[jArray.Count];
            for (int i = 0; i < jArray.Count; i++)
            {
                if (!ToUInt64Value(jArray[i], out array[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool ToFloatArray(this JArray jArray, out float[] array)
        {
            array = new float[jArray.Count];
            for (int i = 0; i < jArray.Count; i++)
            {
                if (!ToFloatValue(jArray[i], out array[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool ToDoubleArray(this JArray jArray, out double[] array)
        {
            array = new double[jArray.Count];
            for (int i = 0; i < jArray.Count; i++)
            {
                if (!ToDoubleValue(jArray[i], out array[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool ToGuidArray(this JArray jArray, out Guid[] array)
        {
            array = new Guid[jArray.Count];
            for (int i = 0; i < jArray.Count; i++)
            {
                if (!ToGuidValue(jArray[i], out array[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool ToStringArray(this JArray jArray, out string[] array)
        {
            array = new string[jArray.Count];
            for (int i = 0; i < jArray.Count; i++)
            {
                if (!ToStringValue(jArray[i], out array[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static JObject ReadFromStreamSync(Stream stream)
        {
            return JObject.Load(new JsonTextReader(new StreamReader(stream)));
        }
        public static bool TryReadFromStreamSync(Stream stream, long length, [NotNullWhen(true)] out JObject? json)
        {
            return TryReadFromStreamSync(stream, out json);
        }

        public static bool TryReadFromStreamSync(Stream stream, [NotNullWhen(true)] out JObject? json)
        {
            try
            {
                json = JObject.Load(new JsonTextReader(new StreamReader(stream)));
            }
            catch (JsonReaderException)
            {
                json = null;
                return false;
            }
            finally
            {
                stream.Dispose();
            }

            return true;
        }
    }
}
