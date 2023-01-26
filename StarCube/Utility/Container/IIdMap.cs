using System;
using System.Collections.Generic;
using System.Text;

namespace StarCube.Utility.Container
{
    public interface IIdMap<T> : IEnumerable<T>
        where T : class
    {
        /// <summary>
        /// 通过值获取 id
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int IdFor(T value);

        /// <summary>
        /// 通过 id 获取值
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T ValueFor(int id);

        /// <summary>
        /// 已存储的值的数量
        /// </summary>
        public int Count { get; }
    }
}
