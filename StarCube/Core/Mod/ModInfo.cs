using System.Reflection;

namespace StarCube.Core.Mod
{
    public class ModInfo
    {

        public readonly string modid;

        public readonly string path;

        public readonly Assembly assembly;

        public readonly IMod modClass;

        internal ModInfo(string modid, string path, Assembly assembly, IMod modClass)
        {
            this.modid = modid;
            this.path = path;
            this.assembly = assembly;
            this.modClass = modClass;
        }
    }
}
