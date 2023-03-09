using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using StarCube.Utility;

namespace StarCube.Core.State.Property
{
    /// <summary>
    /// BooleanStateProperty : 只有两种值 false/true
    /// </summary>
    public sealed class BooleanStateProperty : StateProperty<bool>
    {
        public static BooleanStateProperty Create(StringID id)
        {
            return new BooleanStateProperty(id);
        }

        public static readonly ImmutableArray<bool> BOOLEAN_STATE_PROPERTY_VALUES = new bool[2] { false, true }.ToImmutableArray();

        public BooleanStateProperty(StringID id) : base(id, 2) { }


        public sealed override IEnumerable<bool> Values => BOOLEAN_STATE_PROPERTY_VALUES;

        public sealed override bool ValueIsValid(bool value)
        {
            return true;
        }

        public sealed override bool GetValueByIndex(int index)
        {
            return BOOLEAN_STATE_PROPERTY_VALUES[index];
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
