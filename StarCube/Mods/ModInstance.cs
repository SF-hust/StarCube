using System;
using System.Reflection;

namespace StarCube.Mods
{
    public sealed class ModInstance
    {
        public ModInstance(ModInfo modInfo, Assembly assembly, Type modClass, Mod mod)
        {
            this.modInfo = modInfo;
            this.assembly = assembly;
            this.modClass = modClass;
            this.mod = mod;
        }

        public readonly ModInfo modInfo;

        public readonly Assembly assembly;

        public readonly Type modClass;

        public readonly Mod mod;
    }
}
