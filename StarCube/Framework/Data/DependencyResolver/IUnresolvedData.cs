using System.Collections.Generic;

namespace StarCube.Framework.Data.DependencyResolver
{
    /// <summary>
    /// 表示一份尚未解析依赖关系的数据
    /// </summary>
    /// <typeparam name="K">数据的键类型</typeparam>
    /// <typeparam name="UD">未解析数据的类型</typeparam>
    public interface IUnresolvedData<K, UD>
        where UD : class, IUnresolvedData<K, UD>
    {
        /// <summary>
        /// 数据的 Key
        /// </summary>
        public K Key { get; }

        /// <summary>
        /// 对此数据对象本身的引用
        /// </summary>
        public UD UnresolvedData { get; }

        /*
         * 依赖是一份数据的定义中对另一份数据的引用，用所引用数据的键表示
         * 依赖分为必需依赖和非必需依赖
         * 如果解析时，某个必需依赖所引用的数据不存在(即依赖缺失)，则解析失败
         * 而当某个非必需依赖不存在时，解析器会忽略这个依赖
         * 循环依赖必将导致解析失败，即使循环依赖环中某些或所有依赖为非必需依赖
         * 
         * 对于目前版本的解析器而言，必需依赖里、非必需依赖里、必需依赖与非必需依赖间均允许存在重复
         * 对于前两种情况，其相当于相同的依赖只存在一份，对于第三种情况，相当于这个依赖只存在于必需依赖中
         * 重复的依赖没有意义，应尽可能避免
         */

        /// <summary>
        /// 必需依赖
        /// </summary>
        public IEnumerable<K> RequiredDependencies { get; }

        /// <summary>
        /// 非必需依赖
        /// </summary>
        public IEnumerable<K> OptionalDependencies { get; }
    }
}
