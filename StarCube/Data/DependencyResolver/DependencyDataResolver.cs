using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using StarCube.Data.DependencyResolver;
using StarCube.Resource;

namespace StarCube.Data
{
    public class DependencyDataResolver<K, UD, RD>
        where UD : class, IUnresolvedData<K, UD>
        where RD : class
    {
        private readonly IEnumerable<UD> unresolvedData;
        private readonly IResolvedDataBuilder<K, UD, RD> resolvedDataBuilder;

        public DependencyDataResolver(IEnumerable<UD> unresolvedData, IResolvedDataBuilder<K, UD, RD> resolvedDataBuilder)
        {
            this.unresolvedData = unresolvedData;
            this.resolvedDataBuilder = resolvedDataBuilder;
        }

        /// <summary>
        /// 解析数据间引用并构建解析后的数据
        /// </summary>
        /// <param name="useMultiThread"></param>
        /// <param name="resolvedData"></param>
        /// <returns>如果数据间有重复 key、循环引用或者缺失引用，返回 false</returns>
        public bool BuildResolvedData([NotNullWhen(true)] out Dictionary<K, RD>? resolvedData, bool useMultiThread)
        {
            resolvedData = null;
            ConcurrentDictionary<K, RD> tempResolvedData = new ConcurrentDictionary<K, RD>();
            if (!ResolvedDataDependencies(out List<List<UD>> phases))
            {
                return false;
            }

            foreach (List<UD> phase in phases)
            {
                if (useMultiThread)
                {
                    BuildPhaseMultiThread(phase, tempResolvedData);
                }
                else
                {
                    BuildPhaseSingleThread(phase, tempResolvedData);
                }
            }

            resolvedData = new Dictionary<K, RD>();
            foreach (KeyValuePair<K, RD> pairs in tempResolvedData)
            {
                resolvedData.Add(pairs.Key, pairs.Value);
            }

            return true;
        }

        /// <summary>
        /// 解析数据间的引用
        /// </summary>
        /// <param name="resolvedPhases">分层的数据间的依赖关系，其中每一个 phase 都只依赖于前面 phase 的数据</param>
        /// <returns>如果数据间有重复 key、循环依赖或者缺失依赖，返回 false</returns>
        private bool ResolvedDataDependencies(out List<List<UD>> resolvedPhases)
        {
            Dictionary<K, UD> unresolved = new Dictionary<K, UD>();
            resolvedPhases = new List<List<UD>>();

            // 检查是否有重复 key
            foreach (UD data in unresolvedData)
            {
                if (!unresolved.TryAdd(data.Key, data))
                {
                    return false;
                }
            }

            Dictionary<K, UD> resolved = new Dictionary<K, UD>();
            List<UD> newPhase = new List<UD>();

            // 每次循环找出依赖树中更深的一层
            while (unresolved.Count > 0)
            {
                newPhase.Clear();

                // 找出所有只依赖于已解析完数据的数据
                foreach (UD data in unresolvedData)
                {
                    // 是否所有必需依赖都已解析过
                    bool allRequiredDependencyResolved = true;
                    foreach (K key in data.RequiredDependencies)
                    {
                        if(resolved.ContainsKey(key))
                        {
                            continue;
                        }
                        // 出现缺失的必需依赖则解析失败
                        if (!unresolved.ContainsKey(key))
                        {
                            return false;
                        }
                        allRequiredDependencyResolved = false;
                        break;
                    }

                    // 是否所有非必需依赖都已解析过，或者是无效引用
                    bool allExistingOptionalDependencyResolved = true;
                    foreach (K key in data.OptionalDependencies)
                    {
                        if (unresolved.ContainsKey(key))
                        {
                            allExistingOptionalDependencyResolved = false;
                            break;
                        }
                    }

                    if (allRequiredDependencyResolved && allExistingOptionalDependencyResolved)
                    {
                        newPhase.Add(data);
                    }
                }

                // 存在循环依赖则失败
                if (newPhase.Count == 0)
                {
                    return false;
                }

                foreach (UD data in newPhase)
                {
                    resolved.Add(data.Key, data);
                    unresolved.Remove(data.Key);
                }

                resolvedPhases.Add(newPhase);
            }

            return true;
        }

        private bool BuildPhaseSingleThread(List<UD> phase, IDictionary<K, RD> resolvedData)
        {
            RD? resolvedDataGetter(K key) => resolvedData.TryGetValue(key, out RD value) ? value : null;

            foreach (UD data in phase)
            {
                if (!resolvedDataBuilder.BuildResolvedData(data, resolvedDataGetter, out RD? resolved))
                {
                    return false;
                }
                resolvedData.TryAdd(data.Key, resolved);
            }

            return true;
        }

        private bool BuildPhaseMultiThread(List<UD> phase, IDictionary<K, RD> resolvedData)
        {
            throw new NotImplementedException();
        }
    }
}
