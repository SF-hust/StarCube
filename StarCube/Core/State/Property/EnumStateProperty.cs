using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;

using StarCube.Data;

namespace StarCube.Core.State.Property
{
    /// <summary>
    /// EnumStateProperty<T> : 取值为枚举类型，T 是对应的枚举类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// 注意在解析字符串与将值转为字符串时会使用全小写，因此使用的枚举不可以存在相同的成员名(在忽略大小写的情况下)
    public class EnumStateProperty<T> : StateProperty<T>
        where T : struct, Enum
    {
        /// <summary>
        /// 创建一个 EnumStateProperty，取值范围为枚举的成员，顺序为枚举内成员的声明顺序
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static EnumStateProperty<T> Create(StringID id)
        {
            Array array = Enum.GetValues(typeof(T));
            Debug.Assert(array.Rank == 1);
            List<T> values = new List<T>(array.Length);
            for (int i = 0; i < array.Length; ++i)
            {
                object o = array.GetValue(i);
                Debug.Assert(o is T);
                T v = (T)o;
                values.Add(v);
            }
            return new EnumStateProperty<T>(id, values.ToArray());
        }

        /// <summary>
        /// 创建一个 EnumStateProperty，取值顺序为 values 的声明顺序
        /// </summary>
        /// <param name="id"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static EnumStateProperty<T> Create(StringID id, IEnumerable<T> values)
        {
            if(!CheckedEnumCache.CheckEnumEntries(typeof(T)))
            {
                throw new Exception($"Fail to create EnumProperty : type {typeof(T).FullName} is not for EnumStateProperty");
            }
            List<T> valueList = new List<T>();
            foreach (T value in values)
            {
                if (Enum.IsDefined(typeof(T), value))
                {
                    if (valueList.Contains(value))
                    {
                        throw new Exception($"Fail to create EnumProperty : value \"{value}\" presents twice in values");
                    }
                    valueList.Add(value);
                }
                else
                {
                    throw new Exception($"Fail to create EnumProperty : value \"{value}\" is not in Enum \"{typeof(T).FullName}\"");
                }
            }
            return new EnumStateProperty<T>(id, valueList.ToArray());
        }

        protected EnumStateProperty(StringID id, T[] values) : base(id, values.Length)
        {
            this.values = values;
            keys = new string[values.Length];
            for (int i = 0; i < values.Length; ++i)
            {
                string? key = Enum.GetName(typeof(T), values[i]);
                Debug.Assert(key != null);
                keys[i] = key.ToLower();
            }
        }

        public override IEnumerable<T> Values => values;
        protected T[] values;

        protected string[] keys;

        public override bool ValueIsValid(T value)
        {
            return values.Contains(value);
        }

        public override int GetIndexByValue(T value)
        {
            for (int i = 0; i < values.Length; ++i)
            {
                if (value.Equals(values[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        public override T GetValueByIndex(int index)
        {
            if (index >= 0 && index < countOfValues)
            {
                return values[index];
            }
            throw new IndexOutOfRangeException();
        }

        public override bool TryParseValue(string str, out T value)
        {
            for (int i = 0; i < countOfValues; ++i)
            {
                if (str == keys[i])
                {
                    value = values[i];
                    return true;
                }
            }
            value = default;
            return false;
        }

        public override string ValueToString(T value)
        {
            int index = GetIndexByValue(value);
            return index == -1 ? $"[undefined value(= {value})]" : keys[index];
        }

        public override string ToString()
        {
            return base.ToString() + $", values = {from v in values select Enum.GetName(typeof(T), v).ToLower()}";
        }
    }

    /// <summary>
    /// 已被检查过符合要求的 enum 类型
    /// </summary>
    internal static class CheckedEnumCache
    {
        internal static ConcurrentDictionary<Type, bool> EnumCache = new ConcurrentDictionary<Type, bool>();

        internal static bool CheckEnumEntries(Type type)
        {
            if(!type.IsEnum)
            {
                return false;
            }
            if (Enum.GetUnderlyingType(type) != typeof(int))
            {
                return false;
            }
            if (EnumCache.TryGetValue(type, out bool cachedResult))
            {
                return cachedResult;
            }
            bool checkResult;
            lock(type)
            {
                if (EnumCache.TryGetValue(type, out checkResult))
                {
                    return cachedResult;
                }
                checkResult = true;
                HashSet<string> keys = new HashSet<string>();
                foreach (string key in Enum.GetNames(type))
                {
                    string lower = key.ToLower();
                    if (keys.Contains(lower))
                    {
                        checkResult = false;
                        break;
                    }
                    keys.Add(lower);
                }
                EnumCache[type] = checkResult;
            }
            return checkResult;
        }
    }
}
