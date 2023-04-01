using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using StarCube.Utility;

namespace StarCube.Core.Registries
{
    public class RootRegistry
    {
        public IEnumerable<IRegistry> Registries => registries;

        public void Register(IRegistry registry)
        {
            foreach (IRegistry reg in registries)
            {
                if(reg.ID.Equals(registry.ID))
                {
                    throw new InvalidOperationException();
                }
            }

            registries.Add(registry);
        }

        public bool TryGet(StringID id, [NotNullWhen(true)] out IRegistry? registry)
        {
            foreach (IRegistry reg in registries)
            {
                if (reg.ID.Equals(id))
                {
                    registry = reg;
                    return true;
                }
            }

            registry = null;
            return false;
        }

        public bool TryGet<T>(StringID id, [NotNullWhen(true)] out Registry<T>? registry)
            where T : RegistryEntry<T>
        {
            foreach (IRegistry reg in registries)
            {
                if (reg.ID.Equals(id) && reg is Registry<T> regT)
                {
                    registry = regT;
                    return true;
                }
            }

            registry = null;
            return false;
        }

        public void FireRegistryEvents()
        {
            foreach (IRegistry registry in registries)
            {
                registry.FireRegisterEvent();
            }
        }

        internal RootRegistry()
        {
        }

        private readonly List<IRegistry> registries = new List<IRegistry>();
    }
}
