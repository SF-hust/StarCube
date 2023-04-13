using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace StarCube.Utility.Pools
{
    public sealed class ConcurrentFixedListPool<T>
    {
        public int Size => size;

        /// <summary>
        /// 获取一个数组，注意，数组的内容未知，需自行初始化
        /// </summary>
        /// <returns></returns>
        public List<T> Get()
        {
            if (pool.TryPop(out List<T>? list))
            {
                list.Clear();
                return list;
            }

            return new List<T>(capacity);
        }

        /// <summary>
        /// 将数组返还给对象池
        /// </summary>
        /// <param name="list"></param>
        /// <exception cref="ArgumentException">如果返回的 list 容量与规定容量不匹配</exception>
        public void Release(List<T> list)
        {
            if (list.Capacity != capacity)
            {
                throw new ArgumentException("list capacity dose not match");
            }

            if (pool.Count < size)
            {
                pool.Push(list);
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
                List<T>[] delete = new List<T>[popSize];
                pool.TryPopRange(delete);
            }
        }

        public ConcurrentFixedListPool(int size, int capacity)
        {
            this.size = size;
            this.capacity = capacity;
            pool = new ConcurrentStack<List<T>>();
        }

        public readonly int capacity;

        private volatile int size;
        private readonly ConcurrentStack<List<T>> pool;
    }
}
