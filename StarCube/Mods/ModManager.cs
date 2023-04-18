using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;

namespace StarCube.Mods
{
    public class ModManager
    {
        private static readonly Lazy<ModManager> instance = new Lazy<ModManager>(LoadMods, true);

        public static ModManager Instance => instance.Value;

        private static string modDirectoryPath = string.Empty;

        public static string ModDirectoryPath
        {
            get => modDirectoryPath.Length == 0 ? modDirectoryPath : throw new InvalidOperationException("mod directory path dose not set");
            set
            {
                if (modDirectoryPath.Length != 0)
                {
                    throw new InvalidOperationException("mod directory path has been set");
                }
                if (!Directory.Exists(value))
                {
                    throw new ArgumentException("path not exist", nameof(value));
                }
                modDirectoryPath = value;
            }
        }

        private static ModManager LoadMods()
        {
            ModLoader modLoader = new ModLoader(ModDirectoryPath);
            ImmutableArray<ModInstance> modList = modLoader.LoadMods();
            return new ModManager(modList);
        }

        public IEnumerable<Assembly> GameAssemblies => throw new NotImplementedException();

        private ModManager(ImmutableArray<ModInstance> modList)
        {
            this.modList = modList;
            modidToInstance = new Dictionary<string, ModInstance>();
            foreach (ModInstance mod in modList)
            {
                modidToInstance.Add(mod.modInfo.modid, mod);
            }
        }

        public readonly ImmutableArray<ModInstance> modList;

        private readonly Dictionary<string, ModInstance> modidToInstance;
    }
}
