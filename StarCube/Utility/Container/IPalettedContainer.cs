using System.Collections.Generic;

namespace StarCube.Utility.Container
{
    public interface IPalettedContainer<T> : IEnumerable<T>
        where T : class
    {
        public T this[int index] { get; set; }

        public int Length { get; }
    }
}
