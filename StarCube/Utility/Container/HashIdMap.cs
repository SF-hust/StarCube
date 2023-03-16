using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StarCube.Utility.Container
{
    public class HashIDMap<T> : IIDMap<T>
        where T : class
    {
        public int Count => idToValue.Count;

        public bool Add(T value)
        {
            if(!valueToID.TryAdd(value, Count))
            {
                return false;
            }

            idToValue.Add(value);
            return true;
        }

        public int IdFor(T value)
        {
            if (valueToID.TryGetValue(value, out int id))
            {
                return id;
            }
            return -1;
        }

        public T ValueFor(int id)
        {
            return idToValue[id];
        }

        public IEnumerator<T> GetEnumerator()
        {
            return idToValue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public HashIDMap()
        {
        }

        public HashIDMap(IEnumerable<T> values)
        {
            idToValue = values.ToList();
            for (int i = 0; i < idToValue.Count; i++)
            {
                valueToID.Add(idToValue[i], i);
            }
        }

        private readonly Dictionary<T, int> valueToID = new Dictionary<T, int>();
        private readonly List<T> idToValue = new List<T>();
    }
}
