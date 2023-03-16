using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StarCube.Utility.Container
{
    public class IntegerIDMap<T> : IIdMap<T>
        where T : class, IIntegerID
    {
        public int Count => values.Count;

        public IEnumerator<T> GetEnumerator()
        {
            return values.GetEnumerator();
        }

        public int IdFor(T value)
        {
            return value.IntegerID;
        }

        public T ValueFor(int id)
        {
            return values[id];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IntegerIDMap(IEnumerable<T> values)
        {
            this.values = values.ToList();
        }

        private readonly List<T> values;
    }
}
