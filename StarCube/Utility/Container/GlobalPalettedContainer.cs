using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace StarCube.Utility.Container
{
    public class GlobalPalettedContainer<T> : IPalettedContainer<T>
        where T : class
    {
        public T this[int index]
        {
            get => idMap.ValueFor(data[index]);
            set => data[index] = idMap.IdFor(value);
        }

        public int Length => data.Length;

        public IEnumerator<T> GetEnumerator()
        {
            return (from id in data select idMap.ValueFor(id)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public GlobalPalettedContainer(IIDMap<T> idMap, int size)
        {
            this.idMap = idMap;
            data = new int[size];
        }

        public readonly int[] data;

        private readonly IIDMap<T> idMap;
    }
}
