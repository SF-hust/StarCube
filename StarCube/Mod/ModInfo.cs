using System.Collections.Generic;

namespace StarCube.Mod
{
    public class ModInfo
    {
        internal ModInfo(string modid, string path, List<string> dependencies)
        {
            this.modid = modid;
            this.path = path;
            this.dependencies = dependencies;
        }

        public readonly string modid;

        public readonly string path;

        public readonly List<string> dependencies;
    }
}
