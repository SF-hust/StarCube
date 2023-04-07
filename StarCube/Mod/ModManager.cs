using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace StarCube.Mod
{
    public class ModManager
    {
        private static ModManager? instance = null;

        public static ModManager Instance => instance ?? throw new NullReferenceException();

        public static void LoadMods(string modDirectoryPath)
        {
            ModLoader modLoader = new ModLoader(modDirectoryPath);
            if(!modLoader.TryLoadMods(out ImmutableArray<ModInstance> modList))
            {
                throw new Exception();
            }

            instance = new ModManager(modList);
        }

        private static bool TryLoadInactiveModList(string modDirectoryPath, out List<string> inactiveModids)
        {
            inactiveModids = new List<string>();
            return false;
        }

        private ModManager(ImmutableArray<ModInstance> modList)
        {
            this.modList = modList;
            modidToInstance = new Dictionary<string, ModInstance>();
            foreach (ModInstance mod in modList)
            {
                modidToInstance.Add(mod.modData.modid, mod);
            }
        }

        public readonly ImmutableArray<ModInstance> modList;

        private readonly Dictionary<string, ModInstance> modidToInstance;
    }
}
