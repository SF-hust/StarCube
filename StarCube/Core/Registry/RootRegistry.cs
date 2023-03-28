using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using StarCube.Utility;

namespace StarCube.Core.Registry
{
    public class RootRegistry
    {
        public IEnumerable<Registry> Registries => registries;

        public bool Register(Registry registry)
        {
            foreach (Registry reg in registries)
            {
                if(reg.id.Equals(registry.id))
                {
                    return false;
                }
            }

            registries.Add(registry);

            return true;
        }

        public bool TryGet(StringID id, [NotNullWhen(true)] out Registry? registry)
        {
            foreach (Registry reg in registries)
            {
                if (reg.id.Equals(id))
                {
                    registry = reg;
                    return true;
                }
            }

            registry = null;
            return false;
        }

        public bool TryGet<T>(StringID id, [NotNullWhen(true)] out Registry<T>? registry)
            where T : class, IRegistryEntry<T>
        {
            foreach (Registry reg in registries)
            {
                if (reg.id.Equals(id) && reg is Registry<T> regi)
                {
                    registry = regi;
                    return true;
                }
            }

            registry = null;
            return false;
        }

        public void FireRegistryEvents()
        {
            foreach (Registry registry in registries)
            {
                registry.FireRegisterEvent();
            }
        }

        internal RootRegistry()
        {
        }

        private readonly List<Registry> registries = new List<Registry>();
    }
}
