using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using StarCube.Data.Loading;
using StarCube.Data.Loading.Attributes;
using StarCube.Utility;

namespace StarCube.Data
{
    public static class DataManager
    {
        private static bool initialized = false;

        private static bool bootstrapLoaded = false;

        private static readonly Dictionary<StringID, DataLoader> idToDataLoader = new Dictionary<StringID, DataLoader>();

        private static readonly Dictionary<StringID, List<StringID>> idToFollows = new Dictionary<StringID, List<StringID>>();

        private static readonly List<DataLoader> dataLoaderList = new List<DataLoader>();

        public static void Init(IEnumerable<Assembly> assemblies)
        {
            if (initialized)
            {
                throw new InvalidOperationException("can't call DataManager.Init() twice");
            }

            IEnumerable<Type> dataLoaderTypes = assemblies
                .SelectMany((asm) => asm.GetCustomAttributes<RegisterDataLoaderAttribute>())
                .Select((attr) => attr.type);
            foreach (Type type in dataLoaderTypes)
            {
                if(!type.IsSubclassOf(typeof(DataLoader)))
                {
                    throw new ArgumentException($"type {type.FullName} is not subclass of DataLoader");
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
            }

            if (!Utility.Functions.DependencyResolver.TryResolveDependency(
                idToDataLoader,
                (loader) => loader.id,
                (loader) => idToFollows[loader.id],
                out List<DataLoader> resolved))
            {
                dataLoaderList.AddRange(resolved);
            }

            initialized = true;
        }



        public static void RunAll(DataLoadingContext context)
        {
            if(bootstrapLoaded)
            {
                throw new InvalidOperationException("can't call DataManager.RunAll() twice");
            }

            dataLoaderList.ForEach((loader) => loader.Run(context));

            bootstrapLoaded = true;
        }


        public static bool Reload(StringID id, DataLoadingContext context)
        {


            return true;
        }
    }
}
