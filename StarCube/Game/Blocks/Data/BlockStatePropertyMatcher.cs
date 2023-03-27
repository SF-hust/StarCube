using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using StarCube.Core.State.Property;

namespace StarCube.Game.Blocks.Data
{
    /// <summary>
    /// 表示一个用于与 BlockState 所含有的属性进行匹配的匹配器
    /// </summary>
    public readonly struct BlockStatePropertyMatcher
    {
        /// <summary>
        /// 空匹配，可以匹配任意 BlockState
        /// </summary>
        public static readonly BlockStatePropertyMatcher ANY = new BlockStatePropertyMatcher(new List<KeyValuePair<string, List<string>>>());

        public static bool TryParseFromJson(JObject json, out BlockStatePropertyMatcher matcher)
        {
            matcher = ANY;

            List<KeyValuePair<string, List<string>>> propertyNameToValueStrings = new List<KeyValuePair<string, List<string>>>();
            foreach (var pair in json)
            {
                List<string> valueStrings = new List<string>();
                // 为单个值，这个属性只匹配一个值
                if (pair.Value is JValue singleValue)
                {
                    valueStrings.Add(singleValue.ToString());
                }
                // 为数组，这个属性可以匹配多个值
                else if (pair.Value is JArray array)
                {
                    foreach (JToken token in array)
                    {
                        if (token is JValue jValue)
                        {
                            valueStrings.Add(jValue.ToString());
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                // 格式错误
                else
                {
                    return false;
                }

                propertyNameToValueStrings.Add(new KeyValuePair<string, List<string>>(pair.Key, valueStrings));
            }

            matcher = new BlockStatePropertyMatcher(propertyNameToValueStrings);
            return true;
        }


        /// <summary>
        /// 尝试将某个 BlockState 的属性与这个 Matcher 进行匹配
        /// </summary>
        /// <param name="blockState"></param>
        /// <returns>匹配是否成功</returns>
        public bool Match(BlockState blockState)
        {
            // 必须所有的属性均匹配成功，BlockState 匹配才成功
            foreach (KeyValuePair<string, List<string>> pair in propertyNameToValueStrings)
            {
                int propertyIndex = blockState.propertyList.IndexOf(pair.Key);
                // 不存在对应名字的属性则匹配失败
                if (propertyIndex < 0)
                {
                    return false;
                }

                StateProperty property = blockState.propertyList[propertyIndex].property;
                int valueIndex = blockState.propertyList[propertyIndex].valueIndex;
                // 对于每个属性，任一值匹配成功，则此属性匹配成功
                bool valueMatch = false;
                foreach (string valueString in pair.Value)
                {
                    if (property.ParseToIndex(valueString) == valueIndex)
                    {
                        valueMatch = true;
                        break;
                    }
                }
                if (!valueMatch)
                {
                    return false;
                }
            }
            return true;
        }

        public BlockStatePropertyMatcher(List<KeyValuePair<string, List<string>>> propertyNameToValueStrings)
        {
            this.propertyNameToValueStrings = propertyNameToValueStrings;
        }

        public readonly List<KeyValuePair<string, List<string>>> propertyNameToValueStrings;
    }
}
