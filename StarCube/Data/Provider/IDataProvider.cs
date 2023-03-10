using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using StarCube.Utility;
using StarCube.Data.Loading;
using StarCube.Data.DependencyResolver;

namespace StarCube.Data.Provider
{
    public interface IDataProvider
    {
        /// <summary>
        /// 找到相对数据根路径为 {modid}/{registry}/{path}.* 的数据文件
        /// </summary>
        /// <param name="modid"> 数据 id 的 modid </param>
        /// <param name="registry"> 数据 registry 的 path </param>
        /// <param name="path"> 数据 id 的 path </param>
        /// <param name="dataEntry"></param>
        /// <returns></returns>
        internal protected bool TryGetData(string modid, string registry, string path, out RawDataEntry dataEntry);

        /// <summary>
        /// 找到相对数据根路径为 {modid}/{registry}/{path}.* 的数据文件
        /// </summary>
        /// <param name="modid"> 数据 id 的 modid </param>
        /// <param name="registry"> 数据 registry 的 path </param>
        /// <param name="path"> 数据 id 的 path </param>
        /// <param name="dataEntryChain"></param>
        /// <returns></returns>
        internal protected bool TryGetDataChain(string modid, string registry, string path, List<RawDataEntry> dataEntryChain);

        /// <summary>
        /// 枚举目录 {modid}/{directory}/ 下的所有数据文件
        /// </summary>
        /// <param name="registry">数据 registry 的 path</param>
        /// <param name="directory">要遍历的文件夹相对 modid 文件夹的相对路径</param>
        /// <param name="dataEntries"></param>
        internal protected void EnumerateData(string registry, string directory, List<RawDataEntry> dataEntries);

        /// <summary>
        /// 枚举目录 {modid}/{directory}/ 下的所有数据文件
        /// </summary>
        /// <param name="registry">数据 registry 的 path</param>
        /// <param name="directory">要遍历的文件夹相对 modid 文件夹的相对路径</param>
        /// <param name="dataEntryChains"></param>
        internal protected void EnumerateDataChain(string registry, string directory, List<List<RawDataEntry>> dataEntryChains);
    }

    public static class DataProviderExtension
    {
        public static bool TryLoadData<T>(this IDataProvider dataProvider, StringID dataRegistry, StringID id, IDataReader<T> dataReader, [NotNullWhen(true)] out T? data)
            where T : class
        {
            data = null;
            return dataProvider.TryGetData(id.namspace, dataRegistry.path, id.path, out RawDataEntry dataEntry) &&
                dataReader.TryReadDataFrom(dataEntry.stream, dataEntry.id, out data);
        }

        public static bool TryLoadData<T>(this IDataProvider dataProvider, StringID dataRegistry, string prefix, StringID id, IDataReader<T> dataReader, [NotNullWhen(true)] out T? data)
            where T : class
        {
            string path = Path.Combine(prefix, id.path);
            data = null;
            return dataProvider.TryGetData(id.namspace, dataRegistry.path, path, out RawDataEntry dataEntry) &&
                dataReader.TryReadDataFrom(dataEntry.stream, dataEntry.id, out data);
        }


        public static List<T> EnumerateData<T>(this IDataProvider dataProvider, StringID dataRegistry, IDataReader<T> dataReader)
            where T : class
        {
            List<RawDataEntry> dataEntries = new List<RawDataEntry>();
            dataProvider.EnumerateData(dataRegistry.path, dataRegistry.path, dataEntries);

            List<T> dataList = new List<T>();
            foreach (RawDataEntry dataEntry in dataEntries)
            {
                if (dataReader.TryReadDataFrom(dataEntry.stream, dataEntry.id, out T? data))
                {
                    dataList.Add(data);
                }
            }
            return dataList;
        }

        public static List<T> EnumerateData<T>(this IDataProvider dataProvider, StringID dataRegistry, string prefix, IDataReader<T> dataReader)
            where T : class
        {
            string directory = Path.Combine(dataRegistry.path, prefix);
            List<RawDataEntry> dataEntries = new List<RawDataEntry>();
            dataProvider.EnumerateData(dataRegistry.path, directory, dataEntries);

            List<T> dataList = new List<T>();
            foreach (RawDataEntry dataEntry in dataEntries)
            {
                if (dataReader.TryReadDataFrom(dataEntry.stream, dataEntry.id, out T? data))
                {
                    dataList.Add(data);
                }
            }
            return dataList;
        }


        public static Dictionary<StringID, T> LoadDataWithDependencies<T>(this IDataProvider dataProvider, StringID dataRegistry, IEnumerable<StringID> dataIDs, IDataReader<T> dataReader)
            where T : class, IUnresolvedData<T>
        {
            Dictionary<StringID, T> idToData = new Dictionary<StringID, T>();

            foreach (StringID id in dataIDs)
            {
                if(idToData.ContainsKey(id))
                {
                    continue;
                }

                void LoadWithDependency(StringID dataID)
                {

                    if(!dataProvider.TryLoadData(dataRegistry, dataID, dataReader, out T? data))
                    {
                        return;
                    }

                    idToData.Add(dataID, data);

                    foreach (StringID depID in data.RequiredDependencies)
                    {
                        if(idToData.ContainsKey(depID))
                        {
                            continue;
                        }

                        LoadWithDependency(depID);
                    }
                    foreach (StringID depID in data.OptionalDependencies)
                    {
                        if (idToData.ContainsKey(depID))
                        {
                            continue;
                        }

                        LoadWithDependency(depID);
                    }
                }

                LoadWithDependency(id);
            }

            return idToData;
        }
    }
}
