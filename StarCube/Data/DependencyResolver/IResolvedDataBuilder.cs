using System;
using System.Diagnostics.CodeAnalysis;
using StarCube.Utility;

namespace StarCube.Data.DependencyResolver
{
    /// <summary>
    /// 从未解析数据构建出相应的解析后的数据
    /// </summary>
    /// <typeparam name="K">键类型</typeparam>
    /// <typeparam name="UD">未解析数据的类型</typeparam>
    /// <typeparam name="RD">解析后数据的类型</typeparam>
    public interface IResolvedDataBuilder<UD, RD>
        where UD : class, IUnresolvedData<UD>
        where RD : class
    {
        public delegate bool ResolvedDataGetter(StringID id, [NotNullWhen(true)] out RD? data);

        /// <summary>
        /// 构建一份解析后的数据，注意 : 此方法可能会被多线程调用，各实现者需自行保证实现部分的线程安全
        /// </summary>
        /// <param name="unresolvedData">未解析的数据</param>
        /// <param name="getResolvedData">可通过键获取相应解析后数据的线程安全的委托，此方法被调用时，保证其所有依赖均已被 Build</param>
        /// <param name="resolvedData">构建完成的数据</param>
        /// <returns></returns>
        public bool BuildResolvedData(UD unresolvedData, ResolvedDataGetter getResolvedData, [NotNullWhen(true)] out RD? resolvedData);
    }
}
