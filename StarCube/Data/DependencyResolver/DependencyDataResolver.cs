﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using StarCube.Utility;

namespace StarCube.Data.DependencyResolver
{
    public class DependencyDataResolver<UD, RD>
        where UD : class, IUnresolvedData<UD>
        where RD : class
    {
        public DependencyDataResolver(IEnumerable<UD> unresolvedData, IResolvedDataBuilder<UD, RD> resolvedDataBuilder)
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
        public bool TryBuildResolvedData([NotNullWhen(true)] out Dictionary<StringID, RD>? resolvedData, bool useMultiThread = false)
        {
            resolvedData = null;
            ConcurrentDictionary<StringID, RD> tempResolvedData = new ConcurrentDictionary<StringID, RD>();
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

            resolvedData = new Dictionary<StringID, RD>();
            foreach (KeyValuePair<StringID, RD> pairs in tempResolvedData)
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
            Dictionary<StringID, UD> unresolved = new Dictionary<StringID, UD>();
            resolvedPhases = new List<List<UD>>();

            // 检查是否有重复 key
            foreach (UD data in unresolvedData)
            {
                if (!unresolved.TryAdd(data.ID, data))
                {
                    return false;
                }
            }

            Dictionary<StringID, UD> resolved = new Dictionary<StringID, UD>();
            List<UD> newPhase;

            // 每次循环找出依赖树中更深的一层
            while (unresolved.Count > 0)
            {
                newPhase = new List<UD>();

                // 找出所有只依赖于已解析完数据的数据
                foreach (UD data in unresolved.Values)
                {
                    // 是否所有必需依赖都已解析过
                    bool allRequiredDependencyResolved = true;
                    foreach (StringID key in data.RequiredDependencies)
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
                    foreach (StringID key in data.OptionalDependencies)
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
                    resolved.Add(data.ID, data);
                    unresolved.Remove(data.ID);
                }

                resolvedPhases.Add(newPhase);
            }

            return true;
        }

        private bool BuildPhaseSingleThread(List<UD> phase, IDictionary<StringID, RD> resolvedData)
        {
            foreach (UD data in phase)
            {
                if (!resolvedDataBuilder.BuildResolvedData(data, resolvedData.TryGetValue, out RD? resolved))
                {
                    return false;
                }
                resolvedData.TryAdd(data.ID, resolved);
            }

            return true;
        }

        private bool BuildPhaseMultiThread(List<UD> phase, IDictionary<StringID, RD> resolvedData)
        {
            throw new NotImplementedException();
        }

        private readonly IEnumerable<UD> unresolvedData;
        private readonly IResolvedDataBuilder<UD, RD> resolvedDataBuilder;
    }
}
