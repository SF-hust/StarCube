using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace StarCube.Utility.Container
{
    public class IntegerIDMap<T> : IIDMap<T>
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

        public bool Add(T value)
        {
            if (value.IntegerID == values.Count)
            {
                values.Add(value);
                return true;
            }

            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IntegerIDMap()
        {
            values = new List<T>();
        }

        public IntegerIDMap(IEnumerable<T> values)
        {
            this.values = values.ToList();
        }

        private readonly List<T> values;
    }
}
