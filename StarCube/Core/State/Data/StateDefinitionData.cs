using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using StarCube.Utility;
using StarCube.Data.Loading;
using StarCube.Core.State.Property;
using System.Diagnostics.CodeAnalysis;

namespace StarCube.Core.State.Data
{
    public class StateDefinitionData : IStringID
    {
        public static readonly StringID DataRegistry = StringID.Create(Constants.DEFAULT_NAMESPACE, "state_def");

        public static readonly IDataReader<StateDefinitionData> DataReader = new DataReaderWrapper<StateDefinitionData, JObject>(RawDataReaders.JSON, TryParseFromJson);

        public static bool TryParseFromJson(JObject json, StringID id, [NotNullWhen(true)] out StateDefinitionData? data)
        {
            data = null;

            List<StatePropertyEntry> propertyEntries = new List<StatePropertyEntry>(json.Count);

            foreach (KeyValuePair<string, JToken?> item in json)
            {
                string name = item.Key;
                if(!StringID.IsValidModid(name))
                {
                    return false;
                }

                if (!(item.Value is JArray jArray) || jArray.Count != 2)
                {
                    return false;
                }

                JToken idToken = jArray[0];
                JToken valueToken = jArray[1];

                if(!(idToken is JValue idValue) ||
                    !(idValue.Value is string idString) ||
                    !StringID.TryParse(idString, out StringID propertyID) ||
                    !StateProperty.TryGet(propertyID, out StateProperty? property))
                {
                    return false;
                }

                int valueIndex = property.ParseToIndex(valueToken.ToString());
                if(valueIndex == -1)
                {
                    return false;
                }

                propertyEntries.Add(new StatePropertyEntry(name, property, valueIndex));
            }

            data = new StateDefinitionData(id, propertyEntries);
            return true;
        }

        public StateDefinitionData(StringID id, List<StatePropertyEntry> propertyEntries)
        {
            this.id = id;
            this.propertyEntries = propertyEntries;
        }

        StringID IStringID.ID => id;

        /// <summary>
        /// 数据的 id，也是对应 RegistryEntry 的 id
        /// </summary>
        public readonly StringID id;

        /// <summary>
        /// 属性与其默认值下标
        /// </summary>
        public readonly List<StatePropertyEntry> propertyEntries;
    }
}
