using System.Collections.Immutable;

namespace StarCube.Mods
{
    public sealed class ModInfo
    {
        internal ModInfo(string modid, string path, string description, ImmutableArray<string> dependencies, ImmutableArray<string> followers)
        {
            this.modid = modid;
            this.path = path;
            this.description = description;
            this.dependencies = dependencies;
            this.followers = followers;
        }

        public readonly string modid;

        public readonly string path;

        public readonly string description;

        public readonly ImmutableArray<string> dependencies;

        public readonly ImmutableArray<string> followers;
    }
}
