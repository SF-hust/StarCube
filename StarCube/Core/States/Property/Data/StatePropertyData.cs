using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Newtonsoft.Json.Linq;

using StarCube.Utility;

namespace StarCube.Core.States.Property.Data
{
    public enum StatePropertyType
    {
        Bool,
        Int,
        Enum
    }


    public class StatePropertyDataEntry
    {
        public static bool TryParseFromJson(JObject json, [NotNullWhen(true)] out StatePropertyDataEntry? entry)
        {
            entry = null;

            if (!json.TryGetStringID("id", out StringID id))
            {
                return false;
            }

            StatePropertyType type;
            if (!json.TryGetString("type", out string typeString))
            {
                return false;
            }
            if(typeString.Equals("bool", StringComparison.Ordinal))
            {
                type = StatePropertyType.Bool;
            }
            else if(typeString.Equals("int", StringComparison.Ordinal))
            {
                type = StatePropertyType.Int;
            }
            else if(typeString.Equals("enum", StringComparison.Ordinal))
            {
                type = StatePropertyType.Enum;
            }
            else
            {
                return false;
            }

            int from = 0;
            int to = 0;
            if(type == StatePropertyType.Int)
            {
                if (!json.TryGetInt32("from", out from))
                {
                    return false;
                }
                if (!json.TryGetInt32("to", out to))
                {
                    return false;
                }
            }

            string[] enumValues = Array.Empty<string>();
            if(type == StatePropertyType.Enum)
            {
                if(!json.TryGetArray("values", out JArray? valueArray) || !valueArray.ToStringArray(out string[] values))
                {
                    return false;
                }

                enumValues = values;
            }

            entry = new StatePropertyDataEntry(id, type, from, to, enumValues);
            return true;
        }

        public StateProperty Create()
        {
            if(type == StatePropertyType.Bool)
            {
                return BooleanStateProperty.Create(id);
            }
            else if(type == StatePropertyType.Int)
            {
                return IntegerStateProperty.Create(id, from, to);
            }
            else if(type == StatePropertyType.Enum)
            {
            }

            throw new InvalidOperationException("type is invalid");
        }

        private StatePropertyDataEntry(StringID id, StatePropertyType type, int from, int to, string[] enumValues)
        {
            this.id = id;
            this.type = type;
            this.from = from;
            this.to = to;
            this.enumValues = enumValues;
        }

        public readonly StringID id;
        public readonly StatePropertyType type;
        public readonly int from;
        public readonly int to;
        public readonly string[] enumValues;
    }


    public class StatePropertyData : IStringID
    {
        public static readonly StringID DataRegistry = StringID.Create(Constants.DEFAULT_NAMESPACE, "state_property");

        public static bool TryParseFromJson(JObject json, StringID id, [NotNullWhen(true)] out StatePropertyData? data)
        {
            data = null;
            List<StatePropertyDataEntry> entries = new List<StatePropertyDataEntry>();

            if(json.TryGetArray("definitions", out JArray? entryArray))
            {
                foreach (JToken token in entryArray)
                {
                    if(!(token is JObject entryObject))
                    {
                        return false;
                    }

                    if(!StatePropertyDataEntry.TryParseFromJson(entryObject, out StatePropertyDataEntry? entry))
                    {
                        return false;
                    }

                    entries.Add(entry);
                }
            }

            data = new StatePropertyData(id, entries);
            return true;
        }

        StringID IStringID.ID => id;

        public StatePropertyData (StringID id, List<StatePropertyDataEntry> entries)
        {
            this.id = id;
            this.entries = entries;
        }

        public readonly StringID id;
        public readonly List<StatePropertyDataEntry> entries;
    }
}
