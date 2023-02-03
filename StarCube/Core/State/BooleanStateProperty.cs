using System;
using System.Collections.Generic;

using StarCube.Resource;

namespace StarCube.Core.State
{
    /// <summary>
    /// BooleanStateProperty: 只有两种值 false/true
    /// </summary>
    public sealed class BooleanStateProperty : StateProperty<bool>
    {
        /// <summary>
        /// 创建一个 BooleanStateProperty
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static BooleanStateProperty Create(StringID id)
        {
            return new BooleanStateProperty(id);
        }

        public static readonly IEnumerable<bool> BOOLEAN_STATE_PROPERTY_VALUES = new bool[2] { false, true };

        public BooleanStateProperty(StringID id) : base(id, 2) { }


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


        public sealed override bool TryParseValue(string str, out bool value)
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
    }
}
