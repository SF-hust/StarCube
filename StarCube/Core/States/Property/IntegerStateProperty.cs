using System;
using System.Collections.Generic;
using System.Linq;

using StarCube.Utility;

namespace StarCube.Core.States.Property
{
    /// <summary>
    /// IntegerStateProperty : 可以取指定连续范围内的 int 值(可以是负值)
    /// </summary>
    public class IntegerStateProperty : StateProperty<int>
    {
        /// <summary>
        /// 创建一个 IntegerStateProperty, 取值范围 [from, to]
        /// </summary>
        /// <param name="id"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static IntegerStateProperty Create(StringID id, int from, int to)
        {
            return new IntegerStateProperty(id, from, to);
        }

        /// <summary>
        /// 创建一个 IntegerStateProperty, 取值范围 [0, count - 1]
        /// </summary>
        /// <param name="id"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static IntegerStateProperty Create(StringID id, int count)
        {
            return new IntegerStateProperty(id, 0, count - 1);
        }

        public IntegerStateProperty(StringID id, int from, int to) : base(id, to - from + 1)
        {
            this.from = from;
            this.to = to;
        }

        public readonly int from, to;

        public override IEnumerable<int> Values => Enumerable.Range(from, to);

        public override bool ValueIsValid(int value)
        {
            return value >= from && value <= to;
        }

        public override int GetValueByIndex(int index)
        {
            if (index >= 0 && index < countOfValues)
            {
                return index + from;
            }
            throw new IndexOutOfRangeException();
        }

        public override int GetIndexByValue(int value)
        {
            return ValueIsValid(value) ? value - from : -1;
        }

        public override bool TryParseValue(string str, out int value)
        {
            return int.TryParse(str, out value) && ValueIsValid(value);
        }

        public override string ValueToString(int value)
        {
            return ValueIsValid(value) ? value.ToString() : "!# " + value.ToString();
        }
    }
}
