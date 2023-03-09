using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace StarCube.Utility.Container
{
    public sealed class IntegerIDMapWrapper<T> : IIDMap<T>
        where T : class, IIntegerID
    {
        public static IntegerIDMapWrapper<T> CheckAndCreate(IEnumerable<T> values)
        {
            ImmutableArray<T> valueArray = values.ToImmutableArray();

            for (int i = 0; i < valueArray.Length; ++i)
            {
                if (valueArray[i].IntegerID != i)
                {
                    throw new ArgumentException("check failed : IntegerIDMapWrapper requires values' integer id matching their index");
                }
            }

            return new IntegerIDMapWrapper<T>(valueArray);
        }

        /* IntegerIDMapWrapper 实现 start */
        public int Count => values.Length;

        public int IdFor(T value)
        {
            return value.IntegerID;
        }

        public T ValueFor(int id)
        {
            return values[id];
        }
        /* IntegerIDMapWrapper 实现 end */

        public IEnumerator<T> GetEnumerator()
        {
            return (values as IEnumerable<T>).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IntegerIDMapWrapper(ImmutableArray<T> values)
        {
            this.values = values;
        }

        private readonly ImmutableArray<T> values;
    }
}
