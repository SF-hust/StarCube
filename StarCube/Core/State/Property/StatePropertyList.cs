using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace StarCube.Core.State.Property
{
    public readonly struct StatePropertyEntry
    {
        public readonly string name;
        public readonly StateProperty property;
        public readonly int valueIndex;

        public StatePropertyEntry(string name, StateProperty property, int valueIndex)
        {
            this.name = name;
            this.property = property;
            this.valueIndex = valueIndex;
        }
    }

    /// <summary>
    /// 一个不可变的 StateProperty 与对应值下标列表
    /// </summary>
    public class StatePropertyList : IEnumerable<StatePropertyEntry>
    {
        /// <summary>
        /// 一个 StatePropertyList 中所有 StateProperty 所需二进制位数之和的最大值
        /// </summary>
        public const int MAX_PACKED_BIT_COUNT = 16;

        /// <summary>
        /// 一个 StatePropertyList 中 StateProperty 的最大数量
        /// </summary>
        public const int MAX_STATE_PROPERTY_COUNT = MAX_PACKED_BIT_COUNT;

        /// <summary>
        /// 一个对象的 State 数量的最大值
        /// </summary>
        public const int MAX_STATE_VARIANTS = 1 << MAX_PACKED_BIT_COUNT;


        public static readonly StatePropertyList EMPTY = new StatePropertyList(ImmutableArray<StatePropertyEntry>.Empty);



        /// <summary>
        /// 获取一个 StateProperty 在此列表中的值的下标, 若不存在给定的 StateProperty 则抛出 IndexOutOfRangeException
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public int this[StateProperty property] => GetValueIndex(property);

        /// <summary>
        /// 根据下标返回 StateProperty 及其值的下标
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public StatePropertyEntry this[int index] => propertyEntries[index];

        /// <summary>
        /// 属性列表
        /// </summary>
        public IEnumerable<StateProperty> StateProperties => from propertyIndexPair in propertyEntries select propertyIndexPair.property;

        /// <summary>
        /// 属性列表对应的取值下标列表
        /// </summary>
        public IEnumerable<int> Indices => from propertyIndexPair in propertyEntries select propertyIndexPair.valueIndex;


        public int IndexOf(StateProperty property)
        {
            for (int i = 0; i < propertyEntries.Length; i++)
            {
                if (propertyEntries[i].property == property)
                {
                    return i;
                }
            }
            return -1;
        }

        public int IndexOf(string name)
        {
            for (int i = 0; i < propertyEntries.Length; i++)
            {
                if (propertyEntries[i].name == name)
                {
                    return i;
                }
            }
            return -1;
        }

        public bool Contains(StateProperty property)
        {
            return IndexOf(property) != -1;
        }

        public bool Contains(string name)
        {
            return IndexOf(name) != -1;
        }

        /// <summary>
        /// 获得表中一个 StateProperty 对应的取值的下标
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public int GetValueIndex(StateProperty property)
        {
            int i = IndexOf(property);
            return propertyEntries[i].valueIndex;
        }

        /// <summary>
        /// 获得表中一个 name 对应的取值的下标
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetValueIndex(string name)
        {
            int i = IndexOf(name);
            return propertyEntries[i].valueIndex;
        }

        public override int GetHashCode()
        {
            return packedProperties;
        }

        private static void CalculatePackedPropertiesAndBitCount(ImmutableArray<StatePropertyEntry> propertyEntries, out int packed, out int bitCount)
        {
            packed = 0;
            bitCount = 0;
            foreach (StatePropertyEntry entry in propertyEntries)
            {
                packed |= entry.valueIndex << bitCount;
                bitCount += entry.property.bitCount;
            }
        }


        /* ~ IEnumerable<StatePropertyEntry> 接口实现 start ~ */
        public IEnumerator<StatePropertyEntry> GetEnumerator()
        {
            return (propertyEntries as IEnumerable<StatePropertyEntry>).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        /* ~ IEnumerable<StatePropertyEntry> 接口实现 end ~ */

        private StatePropertyList(ImmutableArray<StatePropertyEntry> propertyEntries)
        {
            this.propertyEntries = propertyEntries;
            CalculatePackedPropertiesAndBitCount(propertyEntries, out packedProperties, out packedProperties);
        }

        /// <summary>
        /// 所有的 StatePropertyEntry
        /// </summary>
        public readonly ImmutableArray<StatePropertyEntry> propertyEntries;

        /// <summary>
        /// StateProperty 数量
        /// </summary>
        public int Count => propertyEntries.Length;

        /// <summary>
        /// 所有属性值打包成的整数
        /// </summary>
        public readonly int packedProperties;

        /// <summary>
        /// 属性值打包完成后所需占用的二进制位数
        /// </summary>
        public readonly int packedBitCount;


        public class Builder
        {
            public static Builder Create()
            {
                return new Builder();
            }

            /// <summary>
            /// 添加一个属性，添加已经添加过的属性将导致异常
            /// </summary>
            /// <param name="property"></param>
            /// <exception cref="Exception"></exception>
            public void AddProperty(string name, StateProperty property)
            {
                if (isBuilding)
                {
                    throw new Exception("Can't add property in building");
                }

                // 检查是否有重复
                foreach (KeyValuePair<string, StateProperty> pair in namePropertyList)
                {
                    if(pair.Key == name || pair.Value == property)
                    {
                        throw new Exception($"StateProperty {name}, {property} already exists");
                    }
                }

                namePropertyList.Add(new KeyValuePair<string, StateProperty>(name, property));
            }

            /// <summary>
            /// 开始执行构建，此方法调用后不再允许添加属性
            /// </summary>
            /// <exception cref="Exception"></exception>
            public void StartBuild()
            {
                isBuilding = true;

                remainToBuild = 1;
                foreach (KeyValuePair<string, StateProperty> pair in namePropertyList)
                {
                    remainToBuild *= pair.Value.countOfValues;
                }

                // 检查属性数量或者属性值数量是否过多
                if (namePropertyList.Count > MAX_STATE_PROPERTY_COUNT)
                {
                    throw new Exception($"A StatePropertyList can only hold at most {MAX_STATE_PROPERTY_COUNT} StateProperties");
                }
                if (remainToBuild > MAX_STATE_VARIANTS)
                {
                    throw new Exception($"An owner can only hold at most {MAX_STATE_VARIANTS} variants");
                }

                valueIndexList.AddRange(Enumerable.Repeat(0, namePropertyList.Count));
            }

            /// <summary>
            /// 构建下一个 State 变种对应的属性表
            /// </summary>
            /// <returns></returns>
            public StatePropertyList BuildNext()
            {
                List<StatePropertyEntry> list = new List<StatePropertyEntry>();
                for (int k = 0; k < namePropertyList.Count; ++k)
                {
                    list.Add(new StatePropertyEntry(namePropertyList[k].Key, namePropertyList[k].Value, valueIndexList[k]));
                }

                UpdateValueIndices();

                return new StatePropertyList(list.ToImmutableArray());
            }

            /// <summary>
            /// 跳过当前表的构建，暂时没用
            /// </summary>
            public void SkipBuild()
            {
                UpdateValueIndices();
            }

            private void UpdateValueIndices()
            {
                if (remainToBuild < 1)
                {
                    throw new Exception("All StatePropertyLists for State variants have been built");
                }
                --remainToBuild;

                bool carry = true;
                int i = 0;
                while (carry && i < namePropertyList.Count)
                {
                    ++valueIndexList[i];
                    carry = false;
                    if (valueIndexList[i] == namePropertyList[i].Value.countOfValues)
                    {
                        valueIndexList[i] = 0;
                        carry = true;
                    }
                    ++i;
                }
            }
            
            public Builder()
            {
            }

            private readonly List<KeyValuePair<string, StateProperty>> namePropertyList = new List<KeyValuePair<string, StateProperty>>();

            private readonly List<int> valueIndexList = new List<int>();

            private bool isBuilding = false;

            private int remainToBuild = 0;
        }
    }
}
