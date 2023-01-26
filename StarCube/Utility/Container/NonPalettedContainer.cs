using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace StarCube.Utility.Container
{
    public sealed class NonPalettedContainer<T> : IPalettedContainer<T>
        where T : class
    {
        public NonPalettedContainer(int length, T initValue)
        {
            values = new T[length];
            for (int i = 0; i < length; ++i)
            {
                values[i] = initValue;
            }
        }

        private readonly T[] values;

        public T this[int index] { get => values[index]; set => values[index] = value; }

        public int Length => values.Length;

        public IEnumerator<T> GetEnumerator()
        {
            return (IEnumerator<T>)values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return values.GetEnumerator();
        }
    }
}
