using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using StarCube.Data;
using StarCube.Mod.Attributes;

namespace StarCube.Mod
{
    public class ModManager
    {
        private static readonly ModManager instance = new ModManager();

        public static ModManager Instance => instance;

        private readonly Dictionary<string, ModInfo> mods = new Dictionary<string, ModInfo>();

        private ModManager()
        {
        }

        public void LoadMods(string modDirectoryPath)
        {
            foreach (string modPath in Directory.EnumerateDirectories(modDirectoryPath))
            {
                LoadMod(modPath);
            }
        }

        private bool LoadMod(string modPath)
        {
            string modAssemblyDirectory = Path.Combine(modPath, "/assembly/");
            List<string> modAssemblyPaths = new List<string>();
            foreach (string modAssemblyPath in Directory.EnumerateFiles(modAssemblyDirectory))
            {
                if (string.Equals(Path.GetExtension(modAssemblyPath), ".dll", StringComparison.OrdinalIgnoreCase))
                {
                    modAssemblyPaths.Add(modAssemblyPath);
                }
            }
            if (modAssemblyPaths.Count != 1)
            {
                throw new Exception($"only 1 .dll file can be in /[mod]/assembly/ directory (directory = \"{modAssemblyDirectory}\")");
            }

            Assembly assembly = Assembly.LoadFrom(modAssemblyPaths[0]);
            bool isModFound = false;
            string modid = string.Empty;
            IMod? foundMod = null;
            foreach (Type type in assembly.ExportedTypes)
            {
                ModAttribute modAttribute = type.GetCustomAttribute<ModAttribute>();
                if (modAttribute == null)
                {
                    continue;
                }

                if (isModFound)
                {
                    throw new Exception($"there are more than 1 class with ModAttribute defined in mod(directory = \"{modPath}\")");
                }
                if (!type.IsSubclassOf(typeof(IMod)))
                {
                    throw new Exception($"type \"{type.FullName}\" with ModAttribute does not implement IMod inteface");
                }
                if (!type.IsClass || type.IsGenericType || type.IsAbstract)
                {
                    throw new Exception($"type \"{type.FullName}\" with ModAttribute must be a non-generic and non-abstract public class");
                }

                modid = modAttribute.modid;
                if (!StringID.IsValidNamespace(modAttribute.modid))
                {
                    throw new Exception($"modid \"{modid}\" is invalid");
                }

                List<ConstructorInfo> constructors = type.GetConstructors().ToList();
                if (constructors.Count != 1 || constructors[0].GetParameters().Length > 0)
                {
                    throw new Exception($"there is only 1 constructor with 0 parameter can be in a mod class (modid = \"{modid}\")");
                }

                foundMod = (IMod)constructors[0].Invoke(null);
                if (foundMod == null)
                {
                    throw new Exception($"fail to construct mod instance (modid = \"{modid}\")");
                }

                isModFound = true;
            }
            if (isModFound == false || foundMod == null)
            {
                throw new Exception($"no mod class found (assembly = \"{assembly.GetName()}\")");
            }

            mods.Add(modid, new ModInfo(modid, modPath, assembly, foundMod));
            return true;
        }
    }
}
