using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using StarCube.Utility;
using StarCube.Data;
using StarCube.Data.Loading;
using StarCube.Core.State.Property;
using System.Diagnostics.CodeAnalysis;

namespace StarCube.Core.State.Data
{
    public class StateDefinitionData : IStringID
    {
        public static StringID DataRegistry = StringID.Create(Constants.DEFAULT_NAMESPACE, "state_def");

        public static IDataReader<StateDefinitionData> DataReader = new DataReaderWrapper<StateDefinitionData, JObject>(JsonHelper.TryReadFromStreamSync, TryParseFromJson);

        public static bool TryParseFromJson(JObject json, StringID id, [NotNullWhen(true)] out StateDefinitionData? data)
        {
            data = null;

            List<KeyValuePair<StateProperty, int>> propertyToDefaultValue = new List<KeyValuePair<StateProperty, int>>(json.Count);

            foreach (KeyValuePair<string, JToken?> item in json)
            {
                if (item.Value == null)
                {
                    return false;
                }

                if (!StringID.TryParse(item.Key, out StringID propertyID) || !StateProperties.TryGet(propertyID, out StateProperty? property))
                {
                    return false;
                }

                int valueIndex = property.ParseToIndex(item.Value.ToString());
                if(valueIndex == -1)
                {
                    return false;
                }

                propertyToDefaultValue.Add(new KeyValuePair<StateProperty, int>(property, valueIndex));
            }

            data = new StateDefinitionData(id, propertyToDefaultValue);
            return true;
        }

        public StateDefinitionData(StringID id, List<KeyValuePair<StateProperty, int>> propertyToDefaultValueIndex)
        {
            this.id = id;
            this.propertyToDefaultValueIndex = propertyToDefaultValueIndex;
        }

        /// <summary>
        /// 数据的 id，也是对应 RegistryEntry 的 id
        /// </summary>
        public readonly StringID id;

        /// <summary>
        /// 属性与其默认值下标
        /// </summary>
        public readonly List<KeyValuePair<StateProperty, int>> propertyToDefaultValueIndex;

        public StringID ID => id;
    }
}
