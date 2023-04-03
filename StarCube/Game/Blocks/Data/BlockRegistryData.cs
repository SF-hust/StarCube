using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Newtonsoft.Json.Linq;

using StarCube.Utility;
using StarCube.Data.Loading;
using StarCube.Core.State.Property;

namespace StarCube.Game.Blocks.Data
{
    public readonly struct BlockRegistryDataEntry
    {
        public static bool TryParseFromJson(JObject json, out BlockRegistryDataEntry entry)
        {
            entry = new BlockRegistryDataEntry();

            if(!json.TryGetString("name", out string name))
            {
                return false;
            }

            BlockProperties.Builder builder = new BlockProperties.Builder();
            if (json.TryGetJObject("properties", out JObject? propertyObject))
            {
                if (propertyObject.TryGetBoolean("air", out bool air) && air)
                {
                    builder.Air();
                }

                if (propertyObject.TryGetBoolean("solid", out bool solid) && solid)
                {
                    builder.Solid();
                }

                if (propertyObject.TryGetFloat("hardness", out float hardness))
                {
                    builder.Hardness(hardness);
                }

                if (propertyObject.TryGetFloat("strength", out float strength))
                {
                    builder.Strength(strength);
                }
            }

            List<StatePropertyEntry> stateDefinition = new List<StatePropertyEntry>();
            if (json.TryGetJObject("states", out JObject? stateObject))
            {
                if(!TryParseStateDefinitionFromJson(stateObject, stateDefinition))
                {
                    return false;
                }
            }

            entry = new BlockRegistryDataEntry(name, builder.Build(), stateDefinition);
            return true;
        }

        public static bool TryParseStateDefinitionFromJson(JObject json, List<StatePropertyEntry> stateDefinition)
        {
            foreach (KeyValuePair<string, JToken?> item in json)
            {
                string name = item.Key;
                if (!StringID.IsValidModid(name))
                {
                    return false;
                }

                if (!(item.Value is JArray jArray) || jArray.Count != 2)
                {
                    return false;
                }

                JToken idToken = jArray[0];
                JToken valueToken = jArray[1];

                if (!(idToken is JValue idValue) ||
                    !(idValue.Value is string idString) ||
                    !StringID.TryParse(idString, out StringID propertyID) ||
                    !StateProperty.TryGet(propertyID, out StateProperty? property))
                {
                    return false;
                }

                int valueIndex = property.ParseToIndex(valueToken.ToString());
                if (valueIndex == -1)
                {
                    return false;
                }

                stateDefinition.Add(new StatePropertyEntry(name, property, valueIndex));
            }

            return true;
        }

        public BlockRegistryDataEntry(string name, in BlockProperties properties, List<StatePropertyEntry> stateDefinition)
        {
            this.name = name;
            this.properties = properties;
            this.stateDefinition = stateDefinition;
        }

        public readonly string name;

        public readonly BlockProperties properties;

        public readonly List<StatePropertyEntry> stateDefinition;
    }

    public class BlockRegistryData : IStringID
    {
        /// <summary>
        /// "starcube:registry/block"
        /// </summary>
        public static readonly StringID DataRegistry = StringID.Create(Constants.DEFAULT_NAMESPACE, "registry/block");

        public static readonly IDataReader<BlockRegistryData> DataReader = new DataReaderWrapper<BlockRegistryData, JObject>(RawDataReaders.JSON, TryParseFromJson);

        public static bool TryParseFromJson(JObject json, StringID id, [NotNullWhen(true)] out BlockRegistryData? data)
        {
            data = null;

            List<BlockRegistryDataEntry> entries = new List<BlockRegistryDataEntry>();
            if(json.TryGetArray("entries", out JArray? entryArray))
            {
                foreach (JToken token in entryArray)
                {
                    if (token is JObject entryObject && BlockRegistryDataEntry.TryParseFromJson(entryObject, out BlockRegistryDataEntry entry))
                    {
                        entries.Add(entry);
                        continue;
                    }

                    return false;
                }
            }

            data = new BlockRegistryData(id, entries);
            return true;
        }


        StringID IStringID.ID => id;

        public BlockRegistryData(StringID id, List<BlockRegistryDataEntry> entries)
        {
            this.id = id;
            this.entries = entries;
        }

        public readonly StringID id;

        public readonly List<BlockRegistryDataEntry> entries;
    }
}
