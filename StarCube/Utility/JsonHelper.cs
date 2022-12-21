using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Newtonsoft.Json.Linq;

namespace StarCube.Utility
{
    public static class JsonHelper
    {
        public static bool TryGetBoolean(JObject json, string propertyName, out bool value)
        {
            if (json.TryGetValue(propertyName, out JToken? token) && token is JValue jValue && jValue.Value is bool bValue)
            {
                value = bValue;
                return true;
            }
            value = false;
            return false;
        }

        public static bool TryGetArray(JObject json, string propertyName, [NotNullWhen(true)] out JArray? array)
        {
            if (json.TryGetValue(propertyName, out JToken? token) && token is JArray arr)
            {
                array = arr;
                return true;
            }
            array = null;
            return false;
        }
    }
}
