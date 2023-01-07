using System;
using System.Collections.Generic;

namespace StarCube.Core.State
{
    /// <summary>
    /// BooleanStateProperty: 只有两种值 false/true
    /// </summary>
    public sealed class BooleanStateProperty : StateProperty<bool>
    {
        /// <summary>
        /// 创建一个指定 name 的 BooleanStateProperty
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static BooleanStateProperty Create(string name)
        {
            return new BooleanStateProperty(name);
        }

        public static readonly IEnumerable<bool> BOOLEAN_STATE_PROPERTY_VALUES = new bool[2] { false, true };

        public BooleanStateProperty(string name) : base(name, 2) { }


        public sealed override IEnumerable<bool> Values => BOOLEAN_STATE_PROPERTY_VALUES;

        public sealed override bool ValueIsValid(bool value)
        {
            return true;
        }
        public sealed override bool GetValueByIndex(int index)
        {
            return index switch
            {
                0 => false,
                1 => true,
                _ => throw new IndexOutOfRangeException(),
            };
        }

        public sealed override int GetIndexByValue(bool value)
        {
            return value ? 1 : 0;
        }


        public sealed override bool ParseValue(string str, out bool value)
        {
            if (str.Equals(bool.FalseString))
            {
                value = false;
                return true;
            }
            if (str.Equals(bool.TrueString))
            {
                value = true;
                return true;
            }
            value = false;
            return false;
        }

        public sealed override string ValueToString(bool value)
        {
            return value ? bool.TrueString : bool.FalseString;
        }

        public sealed override bool ValueEquals(StateProperty<bool>? other)
        {
            return true;
        }
    }
}
