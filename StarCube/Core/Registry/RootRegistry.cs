using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using StarCube.Utility;

namespace StarCube.Core.Registry
{
    public class RootRegistry
    {
        public IEnumerable<Registry> Registries => registryList;

        public bool Register(Registry registry)
        {
            if(!registries.TryAdd(registry.id, registry))
            {
                return false;
            }

            registryList.Add(registry);

            return true;
        }

        public bool TryGet(StringID id, [NotNullWhen(true)] out Registry? registry)
        {
            return registries.TryGetValue(id, out registry);
        }

        public bool TryGet<T>(StringID id, [NotNullWhen(true)] out Registry<T>? registry)
            where T : class, IRegistryEntry<T>
        {
            if(registries.TryGetValue(id, out Registry? areg) && areg is Registry<T> reg)
            {
                registry = reg;
                return true;
            }
            registry = null;
            return false;
        }

        public void FireRegistryEvents()
        {
            foreach (Registry registry in registryList)
            {
                registry.FireRegisterEvent();
            }
        }

        internal RootRegistry()
        {
        }

        private readonly Dictionary<StringID, Registry> registries = new Dictionary<StringID, Registry>();

        private readonly List<Registry> registryList = new List<Registry>();
    }
}
