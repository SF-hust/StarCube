using System.Diagnostics.CodeAnalysis;

using Newtonsoft.Json.Linq;

namespace StarCube.Utility
{
    public static class JsonHelper
    {
        public static bool TryGetBoolean(this JObject json, string propertyName, out bool value)
        {
            if (json.TryGetValue(propertyName, out JToken? token) && token is JValue jValue && jValue.Value is bool bValue)
            {
                value = bValue;
                return true;
            }
            value = false;
            return false;
        }

        public static bool TryGetString(this JObject json, string propertyName, out string value)
        {
            if (json.TryGetValue(propertyName, out JToken? token) && token is JValue jValue && jValue.Value is string stringValue)
            {
                value = stringValue;
                return true;
            }
            value = string.Empty;
            return false;
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

        public static bool TryConvertToString(this JToken token, out string str)
        {
            if(token is JValue v && v.Value is string strValue)
            {
                str = strValue;
                return true;
            }
            str = string.Empty;
            return false;
        }
    }
}
