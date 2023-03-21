using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;

namespace StarCube.Mod
{
    public class ModInfo
    {
        public readonly string modid;

        public readonly string path;

        public readonly ImmutableArray<Assembly> assemblies;

        public readonly IMod modClass;

        internal ModInfo(string modid, string path, IEnumerable<Assembly> assemblies, IMod modClass)
        {
            this.modid = modid;
            this.path = path;
            this.assemblies = assemblies.ToImmutableArray();
            this.modClass = modClass;
        }
    }
}
