using System;
using System.Collections.Generic;
using System.Text;

namespace StarCube.Utility.Container
{
    /// <summary>
    /// IdMap 提供了数字 id 与对象间的双向索引，其数字 id 的取值范围为 [0, Count-1]
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IIdMap<T> : IEnumerable<T>
        where T : class
    {
        /// <summary>
        /// 通过值获取 id
        /// </summary>
        /// <param name="value"></param>
        /// <returns>如果 T 不存在于 IdMap 中则返回 -1</returns>
        public int IdFor(T value);

        /// <summary>
        /// 通过 id 获取值，不会检查 id 是否符合范围
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
