using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using StarCube.Core.State.Property;

namespace StarCube.Core.State
{
    /// <summary>
    /// 一个不可变的 StateProperty 与对应值下标列表
    /// </summary>
    public class StatePropertyList
    {
        /// <summary>
        /// 一个 StatePropertyList 中所有 StateProperty 所需二进制位数之和的最大值
        /// </summary>
        public const int MAX_PACKED_BIT_COUNT = 16;

        /// <summary>
        /// 一个 StatePropertyList 中 StateProperty 的最大数量
        /// </summary>
        public const int MAX_STATE_PROPERTY_COUNT = MAX_PACKED_BIT_COUNT;

        public const int MAX_STATE_VARIANTS = 1 << MAX_PACKED_BIT_COUNT;

        public static readonly StatePropertyList EMPTY = new StatePropertyList(ImmutableArray<KeyValuePair<StateProperty, int>>.Empty);

        private StatePropertyList(ImmutableArray<KeyValuePair<StateProperty, int>> propertyIndexPairs)
        {
            this.propertyIndexPairs = propertyIndexPairs;
            CalculatePackedPropertiesAndBitCount(propertyIndexPairs, out packedProperties, out packedProperties);
        }

        /// <summary>
        /// 获取一个 StateProperty 在此列表中的值的下标, 若不存在给定的 StateProperty 则抛出 IndexOutOfRangeException
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public int this[StateProperty property] => Get(property);

        /// <summary>
        /// 根据下标返回 StateProperty 及其值的下标
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public KeyValuePair<StateProperty, int> this[int index] => propertyIndexPairs[index];

        /// <summary>
        /// 属性列表
        /// </summary>
        public IEnumerable<StateProperty> StateProperties => from propertyIndexPair in propertyIndexPairs select propertyIndexPair.Key;

        /// <summary>
        /// 属性列表对应的取值下标列表
        /// </summary>
        public IEnumerable<int> Indices => from propertyIndexPair in propertyIndexPairs select propertyIndexPair.Value;

        /// <summary>
        /// 
        /// </summary>
        public readonly ImmutableArray<KeyValuePair<StateProperty, int>> propertyIndexPairs;

        /// <summary>
        /// 获取已有的 StateProperty 数量
        /// </summary>
        public int PropertyCount => propertyIndexPairs.Length;


        /// <summary>
        /// 获取所有属性值打包成的整数
        /// </summary>
        public readonly int packedProperties;

        /// <summary>
        /// 获取属性值打包完成后所需占用的二进制位数
        /// </summary>
        public readonly int packedBitCount;

        private static void CalculatePackedPropertiesAndBitCount(ImmutableArray<KeyValuePair<StateProperty, int>> propertyIndexPairs, out int packed, out int bitCount)
        {
            packed = 0;
            bitCount = 0;
            foreach (KeyValuePair<StateProperty, int> pair in propertyIndexPairs)
            {
                packed |= pair.Value << bitCount;
                bitCount += pair.Key.bitCount;
            }
        }

        public int FindProperty(StateProperty property)
        {
            return propertyIndexPairs.First((KeyValuePair<StateProperty, int> pair) => property.Equals(pair.Key)).Value;
        }

        public bool ContainsProperty(StateProperty property)
        {
            return FindProperty(property) != -1;
        }

        /// <summary>
        /// 获得表中一个 StateProperty 对应的取值的下标
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public int Get(StateProperty property)
        {
            int i = FindProperty(property);
            return propertyIndexPairs[i].Value;
        }

        public override int GetHashCode()
        {
            return packedProperties;
        }

        public class Builder
        {
            public static Builder Create()
            {
                return new Builder();
            }

            private readonly List<StateProperty> properties = new List<StateProperty>();

            private readonly List<int> valueIndices = new List<int>();

            private bool isBuilding = false;

            private int remainToBuild = 0;

            public Builder()
            {
            }

            /// <summary>
            /// 添加一个属性，添加已经添加过的属性将导致异常
            /// </summary>
            /// <param name="property"></param>
            /// <exception cref="Exception"></exception>
            public void AddProperty(StateProperty property)
            {
                if(isBuilding)
                {
                    throw new Exception("Can't add property when building");
                }
                if (properties.IndexOf(property) != -1)
                {
                    throw new Exception($"StateProperty {property} already exists");
                }
                properties.Add(property);
            }

            /// <summary>
            /// 开始执行构建，此方法调用后不再允许添加属性
            /// </summary>
            /// <exception cref="Exception"></exception>
            public void StartBuild()
            {
                isBuilding = true;
                remainToBuild = 1;
                foreach (StateProperty property in properties)
                {
                    remainToBuild *= property.countOfValues;
                }
                if (properties.Count > MAX_STATE_PROPERTY_COUNT)
                {
                    throw new Exception($"A StatePropertyList can only hold at most {MAX_STATE_PROPERTY_COUNT} StateProperties");
                }
                if (remainToBuild > MAX_STATE_VARIANTS)
                {
                    throw new Exception($"An owner can only hold at most {MAX_STATE_VARIANTS} variants");
                }
                valueIndices.AddRange(Enumerable.Repeat(0, properties.Count));
            }

            /// <summary>
            /// 构建下一个 State 变种对应的属性表
            /// </summary>
            /// <returns></returns>
            public StatePropertyList BuildNext()
            {
                List<KeyValuePair<StateProperty, int>> list = new List<KeyValuePair<StateProperty, int>>();
                for (int k = 0; k < properties.Count; ++k)
                {
                    list.Add(new KeyValuePair<StateProperty, int>(properties[k], valueIndices[k]));
                }

                UpdateValueIndices();

                return new StatePropertyList(list.ToImmutableArray());
            }

            /// <summary>
            /// 跳过此表的构建，暂时没用
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
                while (carry && i < properties.Count)
                {
                    ++valueIndices[i];
                    carry = false;
                    if (valueIndices[i] == properties[i].countOfValues)
                    {
                        valueIndices[i] = 0;
                        carry = true;
                    }
                    ++i;
                }
            }
        }
    }
}
