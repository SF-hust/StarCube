using StarCube.Utility.Functions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;

namespace StarCube.Mod
{
    public class ModLoader
    {
        public static void LoadModAssemblyInZip(string zipPath)
        {
            ZipArchive zip = ZipFile.OpenRead(zipPath);

            List<string> tempFilenames = new List<string>();

            foreach (var entry in zip.Entries)
            {
                if (entry.FullName.StartsWith("libs/", StringComparison.OrdinalIgnoreCase) && entry.FullName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    string temp = Path.GetTempFileName();
                    entry.ExtractToFile(temp, true);
                    tempFilenames.Add(temp);
                }
            }
        }

        public bool TryLoadMods(out ImmutableArray<ModInstance> modList)
        {
            modList = ImmutableArray<ModInstance>.Empty;
            List<ModInstance> mods = new List<ModInstance>();

            string tempDirectory = Path.Combine(modDirectoryPath, "temp/");
            Directory.CreateDirectory(tempDirectory);

            Dictionary<string, ZipArchive> modidToZip = new Dictionary<string, ZipArchive>();
            Dictionary<string, ModInfo> modidToInfo = new Dictionary<string, ModInfo>();
            foreach (string modPath in Directory.EnumerateFiles(modDirectoryPath))
            {
                if (!Path.HasExtension(".zip"))
                {
                    continue;
                }

                ZipArchive modZip = ZipFile.OpenRead(modPath);
                if(!TryReadModInfo(modZip, out ModInfo? modInfo))
                {
                    return false;
                }

                modidToZip.Add(modInfo.modid, modZip);
                modidToInfo.Add(modInfo.modid, modInfo);
            }

            Dictionary<string, HashSet<string>> modidToDependencies = new Dictionary<string, HashSet<string>>();
            foreach (ModInfo info in modidToInfo.Values)
            {
                if (!modidToDependencies.TryGetValue(info.modid, out HashSet<string>? dependencies))
                {
                    dependencies = new HashSet<string>();
                    modidToDependencies[info.modid] = dependencies;
                }

                foreach (string dep in info.dependencies)
                {
                    dependencies.Add(dep);
                }

                foreach (string follower in info.followers)
                {
                    if (!modidToDependencies.TryGetValue(follower, out HashSet<string>? followerDependencies))
                    {
                        followerDependencies = new HashSet<string>();
                        modidToDependencies[follower] = followerDependencies;
                    }

                    followerDependencies.Add(info.modid);
                }
            }

            if(!DependencyResolver.TryResolveDependency(
                modidToInfo,
                (modInfo) => modInfo.modid,
                (modInfo) => modidToDependencies[modInfo.modid],
                out List<ModInfo> resolvedModInfos))
            {
                return false;
            }

            foreach (ZipArchive zip in modidToZip.Values)
            {
                zip.Dispose();
            }

            modList = mods.ToImmutableArray();
            return true;
        }

        private bool TryReadModInfo(ZipArchive modzip, [NotNullWhen(true)] out ModInfo? modInfo)
        {
            modInfo = null;
            return false;
        }

        internal ModLoader(string modDirectoryPath)
        {
            this.modDirectoryPath = modDirectoryPath;
        }

        private readonly string modDirectoryPath;
    }
}
