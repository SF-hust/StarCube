using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using StarCube.Utility;

namespace StarCube.Core.State.Property
{
    /// <summary>
    /// StateProperty 的非泛型版本基类
    /// </summary>
    public abstract class StateProperty : IStringID
    {
        /// <summary>
        /// 一个 StateProperty 取值的最小数量
        /// </summary>
        public const int MIN_VALUE_COUNT = 2;

        /// <summary>
        /// 一个 StateProperty 取值的最大数量
        /// </summary>
        public const int MAX_VALUE_COUNT = 65536;

        private static readonly ConcurrentDictionary<StringID, StateProperty> AllStateProperties = new ConcurrentDictionary<StringID, StateProperty>();

        private static void AddStateProperty(StateProperty stateProperty)
        {
            if(!AllStateProperties.TryAdd(stateProperty.id, stateProperty))
            {
                throw new Exception($"StateProperty ( id = \"{stateProperty.id}\" ) already exists");
            }
        }

        /// <summary>
        /// 获取指定 id 的 StateProperty
        /// </summary>
        /// <param name="id"></param>
        /// <param name="stateProperty"></param>
        /// <returns></returns>
        public static bool TryGetStatePropertyById(StringID id, [NotNullWhen(true)] out StateProperty? stateProperty)
        {
            return AllStateProperties.TryGetValue(id, out stateProperty);
        }

        /// <summary>
        /// 创建一个 StateProperty
        /// </summary>
        /// <param name="name"></param>
        /// <param name="valueCount"></param>
        /// <exception cref="Exception"></exception>
        public StateProperty(StringID id, int valueCount)
        {
            if (valueCount < MIN_VALUE_COUNT || valueCount > MAX_VALUE_COUNT)
            {
                throw new Exception($"value count must be in [{MIN_VALUE_COUNT}, {MAX_VALUE_COUNT}] for a StateProperty");
            }
            this.id = id;
            countOfValues = valueCount;

            int bCount = 0;
            int vCount = valueCount;
            while(vCount > 0)
            {
                vCount >>= 1;
                bCount++;
            }
            bitCount = bCount;
            bitMask = (1 << bitCount) - 1;

            // 将自身添加到表中
            AddStateProperty(this);
        }

        /// <summary>
        /// 属性的 id
        /// </summary>
        public readonly StringID id;

        public StringID ID => id;

        /// <summary>
        /// 属性值的类型
        /// </summary>
        public abstract Type ValuesType { get; }

        /// <summary>
        /// 属性值的下标的类型
        /// </summary>
        public Type IndexType => typeof(int);

        /// <summary>
        /// 此属性可能取值的数量
        /// </summary>
        public readonly int countOfValues;

        /// <summary>
        /// 为表示属性不同取值所需的最少二进制位的数量
        /// </summary>
        public readonly int bitCount;

        /// <summary>
        /// bitCount 所对应的位掩码
        /// </summary>
        public readonly int bitMask;

        /// <summary>
        /// 某个属性值对应的下标是否在此属性可取值范围内
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool IndexIsValid(int index)
        {
            return index >= 0 && index < countOfValues;
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public override string ToString()
        {
            return  $"StateProperty ( id = \"{id}\", value type = {ValuesType} )";
        }

        public abstract string IndexToString(int index);

        public abstract int ParseToIndex(string valueString);
    }

    /// <summary>
    /// StateProperty 的泛型版本基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class StateProperty<T> : StateProperty
        where T : struct
    {
        public StateProperty(StringID id, int valueCount) : base(id, valueCount)
        {
        }

        public sealed override Type ValuesType => typeof(T);

        /// <summary>
        /// 从字符串中解析出值并获取其下标, 解析失败则返回 -1
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public int ParseValueToIndex(string str)
        {
            return TryParseValue(str, out T value) ? GetIndexByValue(value) : -1;
        }

        /// <summary>
        /// 通过下标获取对应值并转化为字符串, 超出范围则抛出 IndexOutOfRangeException
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string ValueIndexToString(int index)
        {
            return ValueToString(GetValueByIndex(index));
        }

        /*
         * 以下是每种具体属性都需要实现的部分
         */

        /// <summary>
        /// 此属性所有的可能取值，应按下标顺序排列
        /// </summary>
        public abstract IEnumerable<T> Values { get; }

        /// <summary>
        /// 此属性是否可以取某值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public abstract bool ValueIsValid(T value);

        /// <summary>
        /// 使用下标取得某个取值范围内的值, 如果下标超出范围则应抛 IndexOutOfRangeException
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public abstract T GetValueByIndex(int index);

        /// <summary>
        /// 获取某个值在属性中对应的下标, 如果不能取到则返回 -1
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public abstract int GetIndexByValue(T value);

        /// <summary>
        /// 从一个 string 中解析出值, 返回值表示是否解析成功, 与 ValueToString() 对应
        /// </summary>
        /// <param name="valueString"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public abstract bool TryParseValue(string valueString, out T value);

        public T ParseValue(string valueString)
        {
            if(TryParseValue(valueString, out T value))
            {
                return value;
            }
            throw new Exception($"parse value failed :(id = \"{id}\", value string = \"{valueString}\")");
        }

        /// <summary>
        /// 从一个 string 中解析出值，并获取其下标，返回 -1 代表解析失败
        /// </summary>
        /// <param name="valueString"></param>
        /// <returns></returns>
        public override int ParseToIndex(string valueString)
        {
            if(TryParseValue(valueString, out T value))
            {
                return GetIndexByValue(value);
            }
            return -1;
        }

        /// <summary>
        /// 将某个取值转换为字符串, 这个字符串应能被 ParseValue() 解析得到这个值,
        /// 值超出范围时应返回一个不能被 ParseValue() 解析的字符串
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public abstract string ValueToString(T value);

        public override string IndexToString(int index)
        {
            if(!IndexIsValid(index))
            {
                return $"invalid index ({index})";
            }
            return ValueToString(GetValueByIndex(index));
        }
    }
}
