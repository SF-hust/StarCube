using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Reflection;

using StarCube.Utility;
using StarCube.Utility.Functions;
using StarCube.Mods.Attributes;

namespace StarCube.Mods
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

        public ImmutableArray<ModInstance> LoadMods()
        {
            List<ModInstance> mods = new List<ModInstance>();

            // 创建暂存 assembly 的临时文件夹
            string modTempDirectoryPath = Path.Combine(modDirectoryPath, "temp");
            Directory.CreateDirectory(modTempDirectoryPath);

            Dictionary<string, string> modidToPath = new Dictionary<string, string>();
            Dictionary<string, ModInfo> modidToInfo = new Dictionary<string, ModInfo>();
            Dictionary<string, Assembly> modidToAssembly = new Dictionary<string, Assembly>();
            foreach (string modPath in Directory.EnumerateFiles(modDirectoryPath))
            {
                if (!Path.HasExtension(".zip"))
                {
                    continue;
                }

                // 读取 mod 信息
                using ZipArchive modZip = ZipFile.OpenRead(modPath);

                ModInfo modInfo = ReadModInfo(modZip);
                Assembly modAssembly = ReadModAssembly(modZip, modTempDirectoryPath);

                modidToPath.Add(modInfo.modid, modPath);
                modidToInfo.Add(modInfo.modid, modInfo);
                modidToAssembly.Add(modInfo.modid, modAssembly);
            }

            // 删除暂存 assembly 的临时文件夹
            Directory.Delete(modTempDirectoryPath, true);

            // 收集 mod 依赖信息
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

            // 解析 mod 依赖
            if(!DependencyResolver.TryResolveDependency(
                modidToInfo,
                (modInfo) => modInfo.modid,
                (modInfo) => modidToDependencies[modInfo.modid],
                out List<ModInfo> resolvedModInfos))
            {
                throw new Exception("resolve mod dependency failed");
            }

            // 构建 modInstance
            foreach (ModInfo info in resolvedModInfos)
            {
                Assembly assembly = modidToAssembly[info.modid];
                RegisterModAttribute? attribute = assembly.GetCustomAttribute<RegisterModAttribute>();
                if (attribute == null)
                {
                    throw new Exception($"assembly in mod {info.modid} does not contain RegisterModAttribute");
                }
                if (!attribute.modid.Equals(info.modid, StringComparison.Ordinal))
                {
                    throw new Exception($"modid in mod {info.modid} does not match");
                }

                Type type = attribute.type;
                if (type.IsGenericType)
                {
                    throw new Exception($"mod class {type.FullName} for mod {info.modid} is generic");
                }
                if (type.IsAbstract)
                {
                    throw new Exception($"mod class {type.FullName} for mod {info.modid} is abstract");
                }
                if (!type.IsSubclassOf(typeof(Mod)))
                {
                    throw new Exception($"mod class {type.FullName} for mod {info.modid} is not subclass of Mod");
                }

                ConstructorInfo? constructorInfo = type.GetConstructor(Array.Empty<Type>());
                if (constructorInfo == null)
                {
                    throw new ArgumentException($"mod class  {type.FullName} must have a default constructor");
                }

                Mod mod = (Mod)constructorInfo.Invoke(Array.Empty<object>());

                mods.Add(new ModInstance(info, assembly, type, mod));
            }

            return mods.ToImmutableArray();
        }

        private ModInfo ReadModInfo(ZipArchive modzip)
        {
            return new ModInfo(Constants.DEFAULT_NAMESPACE, string.Empty, string.Empty, ImmutableArray<string>.Empty, ImmutableArray<string>.Empty);
        }

        private Assembly ReadModAssembly(ZipArchive modzip, string tempDirectory)
        {
            ZipArchiveEntry? assemblyEntry = null;
            foreach (var entry in modzip.Entries)
            {
                if (entry.FullName.StartsWith("assembly/", StringComparison.OrdinalIgnoreCase))
                {
                    if (assemblyEntry != null)
                    {
                        throw new Exception("found more than 1 file in modzip assembly/*");
                    }

                    assemblyEntry = entry;
                }
            }

            if (assemblyEntry == null)
            {
                throw new Exception("no assembly file found in mod zip");
            }

            string assemblyPath = Path.Combine(tempDirectory, assemblyEntry.Name);
            assemblyEntry.ExtractToFile(assemblyPath);
            return Assembly.LoadFile(assemblyPath);
        }

        internal ModLoader(string modDirectoryPath)
        {
            this.modDirectoryPath = modDirectoryPath;
        }

        private readonly string modDirectoryPath;
    }
}
