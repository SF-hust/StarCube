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
        /// 枚举目录 {modid}/{directory}/ 下的所有数据文件
        /// </summary>
        /// <param name="registry">数据 registry 的 path</param>
        /// <param name="directory">要遍历的文件夹相对 modid 文件夹的相对路径</param>
        /// <param name="dataEntries"></param>
        internal protected void EnumerateData(string registry, string directory, List<RawDataEntry> dataEntries);
    }

    public static class DataProviderExtension
    {
        public static bool TryLoadData<T>(this IDataProvider dataProvider, StringID dataRegistry, StringID id, IDataReader<T> dataReader, [NotNullWhen(true)] out T? data)
            where T : class
        {
            data = null;
            return dataProvider.TryGetData(id.namspace, dataRegistry.path, id.path, out RawDataEntry dataEntry) &&
                dataReader.TryReadDataFrom(dataEntry.stream, dataEntry.length, dataEntry.id, out data);
        }

        public static bool TryLoadData<T>(this IDataProvider dataProvider, StringID dataRegistry, string prefix, StringID id, IDataReader<T> dataReader, [NotNullWhen(true)] out T? data)
            where T : class
        {
            string path = Path.Combine(prefix, id.path).Replace(Path.DirectorySeparatorChar, StringID.PATH_SEPARATOR_CHAR);
            data = null;
            return dataProvider.TryGetData(id.namspace, dataRegistry.path, path, out RawDataEntry dataEntry) &&
                dataReader.TryReadDataFrom(dataEntry.stream, dataEntry.length, dataEntry.id, out data);
        }

        public static List<StringID> LoadDataList<T>(this IDataProvider dataProvider, StringID dataRegistry, IEnumerable<StringID> ids, IDataReader<T> dataReader, out List<T> dataList)
            where T : class
        {
            List<StringID> missingDataIDs = new List<StringID>();
            dataList = new List<T>();

            foreach (StringID id in ids)
            {
                if(dataProvider.TryLoadData(dataRegistry, id, dataReader, out T? data))
                {
                    dataList.Add(data);
                }
                else
                {
                    missingDataIDs.Add(id);
                }
            }
            return missingDataIDs;
        }

        public static List<StringID> LoadDataDictionary<T>(this IDataProvider dataProvider, StringID dataRegistry, IEnumerable<StringID> ids, IDataReader<T> dataReader, out Dictionary<StringID, T> dataDictionary)
            where T : class
        {
            List<StringID> missingDataIDs = new List<StringID>();
            dataDictionary = new Dictionary<StringID, T>();

            foreach (StringID id in ids)
            {
                if (dataProvider.TryLoadData(dataRegistry, id, dataReader, out T? data))
                {
                    dataDictionary.Add(id, data);
                }
                else
                {
                    missingDataIDs.Add(id);
                }
            }
            return missingDataIDs;
        }

        public static List<T> EnumerateData<T>(this IDataProvider dataProvider, StringID dataRegistry, IDataReader<T> dataReader)
            where T : class
        {
            List<RawDataEntry> dataEntries = new List<RawDataEntry>();
            dataProvider.EnumerateData(dataRegistry.path, dataRegistry.path, dataEntries);

            List<T> dataList = new List<T>();
            foreach (RawDataEntry dataEntry in dataEntries)
            {
                if (dataReader.TryReadDataFrom(dataEntry.stream, dataEntry.length, dataEntry.id, out T? data))
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
                if (dataReader.TryReadDataFrom(dataEntry.stream, dataEntry.length, dataEntry.id, out T? data))
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

                LoadDataWithDependency(dataProvider, dataRegistry, id, dataReader, idToData);
            }

            return idToData;
        }

        public static List<T> LoadDataWithDependency<T>(this IDataProvider dataProvider, StringID dataRegistry, IEnumerable<StringID> dataIDs, IDataReader<T> dataReader)
            where T : class, IUnresolvedData<T>
        {
            List<T> loadedData = new List<T>();
            // 已加载过的数据的 id 记录，防止重复加载
            HashSet<StringID> loadedDataID = new HashSet<StringID>();

            foreach (StringID id in dataIDs)
            {
                static void LoadDataWithDependency(IDataProvider dataProvider, StringID dataRegistry, StringID id, IDataReader<T> dataReader, HashSet<StringID> loadedDataID, List<T> loadedData)
                {
                    if (loadedDataID.Contains(id))
                    {
                        return;
                    }

                    // 先加载自身
                    if (!dataProvider.TryLoadData(dataRegistry, id, dataReader, out T? data))
                    {
                        return;
                    }

                    // 再处理依赖
                    foreach (StringID depID in data.RequiredDependencies)
                    {
                        if (loadedDataID.Contains(depID))
                        {
                            continue;
                        }

                        LoadDataWithDependency(dataProvider, dataRegistry, depID, dataReader, loadedDataID, loadedData);
                    }
                    foreach (StringID depID in data.OptionalDependencies)
                    {
                        if (loadedDataID.Contains(depID))
                        {
                            continue;
                        }

                        LoadDataWithDependency(dataProvider, dataRegistry, depID, dataReader, loadedDataID, loadedData);
                    }

                    // 最后将自身放进表中，即被依赖项总是在依赖项之前
                    loadedDataID.Add(id);
                    loadedData.Add(data);
                }

                if (loadedDataID.Contains(id))
                {
                    continue;
                }

                LoadDataWithDependency(dataProvider, dataRegistry, id, dataReader, loadedDataID, loadedData);
            }

            return loadedData;
        }

        private static void LoadDataWithDependency<T>(this IDataProvider dataProvider, StringID dataRegistry, StringID id, IDataReader<T> dataReader, Dictionary<StringID, T> idToData)
            where T : class, IUnresolvedData<T>
        {
            if (idToData.ContainsKey(id))
            {
                return;
            }

            if (!dataProvider.TryLoadData(dataRegistry, id, dataReader, out T? data))
            {
                return;
            }

            idToData.Add(id, data);

            foreach (StringID depID in data.RequiredDependencies)
            {
                if (idToData.ContainsKey(depID))
                {
                    continue;
                }

                LoadDataWithDependency(dataProvider, dataRegistry, depID, dataReader, idToData);
            }

            foreach (StringID depID in data.OptionalDependencies)
            {
                if (idToData.ContainsKey(depID))
                {
                    continue;
                }

                LoadDataWithDependency(dataProvider, dataRegistry, depID, dataReader, idToData);
            }
        }
    }
}
