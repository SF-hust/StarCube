using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using StarCube.Utility;
using StarCube.Utility.Logging;
using StarCube.Utility.Functions;
using StarCube.Data.Loading;
using StarCube.Data.Loading.Attributes;

namespace StarCube.Data
{
    public static class DataManager
    {
        private static bool initialized = false;

        private static bool bootstrapLoaded = false;

        private static readonly Dictionary<StringID, DataLoader> idToDataLoader = new Dictionary<StringID, DataLoader>();

        private static readonly Dictionary<StringID, HashSet<StringID>> idToDependencies = new Dictionary<StringID, HashSet<StringID>>();

        private static readonly Dictionary<StringID, HashSet<StringID>> idToFollowers = new Dictionary<StringID, HashSet<StringID>>();

        private static readonly List<DataLoader> dataLoaderChain = new List<DataLoader>();

        private static readonly Dictionary<StringID, List<DataLoader>> idToReloadChain = new Dictionary<StringID, List<DataLoader>>();


        public static void Init(IEnumerable<Assembly> assemblies)
        {
            if (initialized)
            {
                LogUtil.Logger.Error("tries to call DataManager.Init() twice");
                throw new InvalidOperationException("can't call DataManager.Init() twice");
            }

            IEnumerable<Type> dataLoaderTypes = assemblies
                .SelectMany((asm) => asm.GetCustomAttributes<RegisterDataLoaderAttribute>())
                .Select((attr) => attr.type);

            // 检查类型是否符合要求，构造实例
            foreach (Type type in dataLoaderTypes)
            {
                if(!type.IsSubclassOf(typeof(DataLoader)))
                {
                    throw new ArgumentException($"type {type.FullName} is not subclass of DataLoader");
                }

                if (type.IsGenericType)
                {
                    throw new ArgumentException($"type {type.FullName} is generic");
                }

                ConstructorInfo? constructorInfo = type.GetConstructor(Array.Empty<Type>());
                if(constructorInfo == null)
                {
                    throw new ArgumentException($"type {type.FullName} must have a default constructor");
                }

                DataLoader dataLoader = (DataLoader)constructorInfo.Invoke(Array.Empty<object>());
                if(!idToDataLoader.TryAdd(dataLoader.id, dataLoader))
                {
                    throw new ArgumentException($"data loader {dataLoader.id} already exists");
                }

                LogUtil.Logger.Info($"loader \"{dataLoader.id}\" found");
            }

            // 收集各个 loader 的依赖
            foreach (DataLoader loader in idToDataLoader.Values)
            {
                if (!idToDependencies.TryGetValue(loader.id, out HashSet<StringID>? dependencies))
                {
                    dependencies = new HashSet<StringID>();
                    idToDependencies[loader.id] = dependencies;
                }

                if (!idToFollowers.TryGetValue(loader.id, out HashSet<StringID>? followers))
                {
                    followers = new HashSet<StringID>();
                    idToFollowers[loader.id] = followers;
                }

                foreach (StringID dependencyID in loader.Dependencies)
                {
                    dependencies.Add(dependencyID);
                }

                foreach (StringID followerID in loader.Followers)
                {
                    followers.Add(followerID);
                }

                foreach (StringID followerID in loader.Followers)
                {
                    if (!idToDependencies.TryGetValue(followerID, out HashSet<StringID>? followerDependencies))
                    {
                        followerDependencies = new HashSet<StringID>();
                        idToDependencies[followerID] = followerDependencies;
                    }

                    followerDependencies.Add(loader.id);
                }

                foreach (StringID dependencyID in loader.Dependencies)
                {
                    if (!idToFollowers.TryGetValue(dependencyID, out HashSet<StringID>? dependencyFollowers))
                    {
                        dependencyFollowers = new HashSet<StringID>();
                        idToFollowers[dependencyID] = dependencyFollowers;
                    }

                    dependencyFollowers.Add(loader.id);
                }
            }

            // 生成数据加载顺序
            if (!DependencyResolver.TryResolveDependency(
                idToDataLoader,
                (loader) => loader.id,
                (loader) => idToDependencies[loader.id],
                out List<DataLoader> resolved))
            {
                throw new ArgumentException("resolve data loaders failed");
            }
            dataLoaderChain.AddRange(resolved);

            initialized = true;
        }


        public static void RunAll(DataLoadingContext context)
        {
            if(bootstrapLoaded)
            {
                LogUtil.Logger.Error("tries to call DataManager.RunAll() twice");
                throw new InvalidOperationException("can't call DataManager.RunAll() twice");
            }

            foreach (var loader in dataLoaderChain)
            {
                LogUtil.Logger.Info($"loader \"{loader.id}\" running");
                loader.Run(context);
            }

            bootstrapLoaded = true;
        }


        public static bool Reload(StringID id, DataLoadingContext context)
        {
            if(!idToDataLoader.TryGetValue(id, out DataLoader? dataLoader) || !dataLoader.reloadable)
            {
                LogUtil.Logger.Warning($"data loader \"{id}\" not found or not reloadable");
                return false;
            }

            if (!idToReloadChain.TryGetValue(id, out List<DataLoader>? reloadChain))
            {
                reloadChain = new List<DataLoader>();
                Dictionary<StringID, DataLoader> idToReloadDataLoader = new Dictionary<StringID, DataLoader>();
                idToReloadDataLoader.Add(id, dataLoader);
                foreach (StringID followerID in idToFollowers[id])
                {
                    if (!idToDataLoader[followerID].reloadable)
                    {
                        LogUtil.Logger.Warning($"in reloading \"{id}\" : data loader \"{followerID}\" is not reloadable");
                        return false;
                    }
                    idToReloadDataLoader.Add(followerID, idToDataLoader[followerID]);
                }
                if (!DependencyResolver.TryResolveDependency(
                    idToReloadDataLoader,
                    (loader) => loader.id,
                    (loader) => idToDependencies[loader.id],
                    out reloadChain))
                {
                    LogUtil.Logger.Warning($"can't resolve dependencies in reloading \"{id}\"");
                    reloadChain.Clear();
                }

                idToReloadChain.Add(id, reloadChain);
            }

            if (reloadChain.Count == 0)
            {
                LogUtil.Logger.Warning($"fail to reload \"{id}\"");
                return false;
            }

            foreach (DataLoader loader in reloadChain)
            {
                LogUtil.Logger.Info($"reloading \"{id}\"");
                loader.Run(context);
            }

            return true;
        }
    }
}
