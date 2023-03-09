using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StarCube.Utility.Container
{
    public class HashIdMap<T> : IIdMap<T>
        where T : class
    {
        protected readonly Dictionary<T, int> idByValue = new Dictionary<T, int>();
        protected readonly List<T> valueById = new List<T>();

        public HashIdMap(IEnumerable<T> values)
        {
            valueById = values.ToList();
            for (int i = 0; i < valueById.Count; i++)
            {
                idByValue.Add(valueById[i], i);
            }
        }

        public int Count => valueById.Count;

        public int IdFor(T value)
        {
            if (idByValue.TryGetValue(value, out int id))
            {
                return id;
            }
            return -1;
        }

        public T ValueFor(int id)
        {
            if (id < 0 || id >= Count)
            {
                throw new IndexOutOfRangeException();
            }
            return valueById[id];
        }

        public IEnumerator<T> GetEnumerator()
        {
            return valueById.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
