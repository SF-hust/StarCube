using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace StarCube.Mod
{
    public sealed class ModInstance
    {
        public ModInstance(ModInfo modData, Assembly assembly, Type type, IMod mod)
        {
            this.modData = modData;
            this.assembly = assembly;
            this.type = type;
            this.mod = mod;
        }

        public readonly ModInfo modData;

        public readonly Assembly assembly;

        public readonly Type type;

        public readonly IMod mod;
    }
}
