using System;
using System.Collections.Generic;

namespace StarCube.Utility.Functions
{
    public static class DependencyResolver
    {
        public static bool TryResolveDependency<I, T>(IDictionary<I, T> idToValue, Func<T, I> idGetter, Func<T, IEnumerable<I>> dependencyGetter, out List<T> resolvedList)
        {
            resolvedList = new List<T>();
            Dictionary<I, T> unresolved = new Dictionary<I, T>(idToValue);
            List<I> toRemoveList = new List<I>();

            while (unresolved.Count > 0)
            {
                int resolvedCount = 0;
                foreach (T value in unresolved.Values)
                {
                    bool dependencyResolved = true;
                    foreach(I id in dependencyGetter(value))
                    {
                        if (unresolved.ContainsKey(id))
                        {
                            dependencyResolved = false;
                        }
                    }

                    if(!dependencyResolved)
                    {
                        continue;
                    }

                    resolvedList.Add(value);
                    toRemoveList.Add(idGetter(value));
                }

                foreach (I id in toRemoveList)
                {
                    unresolved.Remove(id);
                }
                toRemoveList.Clear();

                if(resolvedCount == 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
