using System;
using System.Collections.Concurrent;

namespace StarCube.Utility.Pools
{
    /// <summary>
    /// 一个用于容纳固定尺寸数组的，可以多线程 Get Release 的简单对象池；
    /// 注意由于实现方式，多线程操作时池中对象数量可能会略微超出 size 限制
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class ConcurrentFixedArrayPool<T>
    {
        public int Size => size;

        /// <summary>
        /// 获取一个数组，注意，数组的内容未知，需自行初始化
        /// </summary>
        /// <returns></returns>
        public T[] Get()
        {
            if (pool.TryPop(out T[]? array))
            {
                return array;
            }

            return new T[length];
        }

        /// <summary>
        /// 将数组返还给对象池
        /// </summary>
        /// <param name="array"></param>
        /// <exception cref="ArgumentException">如果返回的数组长度与规定长度不匹配</exception>
        public void Release(T[] array)
        {
            if (array.Length != length)
            {
                throw new ArgumentException("array length dose not match");
            }

            if (pool.Count < size)
            {
                pool.Push(array);
            }
        }

        /// <summary>
        /// 重新设置对象池尺寸
        /// </summary>
        /// <param name="newSize"></param>
        public void Resize(int newSize)
        {
            size = newSize;
            int popSize = pool.Count - newSize;
            if (popSize > 0)
            {
                T[][] delete = new T[popSize][];
                pool.TryPopRange(delete);
            }
        }

        public ConcurrentFixedArrayPool(int size, int length)
        {
            this.size = size;
            this.length = length;
            pool = new ConcurrentStack<T[]>();
        }

        public readonly int length;

        private volatile int size;
        private readonly ConcurrentStack<T[]> pool;
    }
}
