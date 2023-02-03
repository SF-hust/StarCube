using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using StarCube.Resource;

namespace StarCube.Core.Registry
{
    public class RootRegistry
    {
        private readonly Dictionary<StringID, Registry> registries = new Dictionary<StringID, Registry>();

        public IEnumerable<Registry> Registries => registries.Values;

        internal RootRegistry()
        {
        }

        public static readonly StringKey key = StringKey.Create(Registry.RegistryRegistry, Registry.RegistryRegistry);

        public bool Register(Registry registry)
        {
            return registries.TryAdd(registry.id, registry);
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
    }
}
