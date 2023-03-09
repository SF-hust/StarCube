using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using StarCube.Data.Loading;
using StarCube.Data.DependencyResolver;

namespace StarCube.Data.Provider
{
    public interface IDataProvider
    {
        /// <summary>
        /// 找到相对路径为 {directory}/{entryName}.* 的数据文件项
        /// </summary>
        /// <param name="modid"></param>
        /// <param name="registry"></param>
        /// <param name="directory"></param>
        /// <param name="entryName"></param>
        /// <param name="dataEntry"></param>
        /// <returns></returns>
        internal bool TryGetData(string modid, string registry, string directory, string entryName, out RawDataEntry dataEntry);

        /// <summary>
        /// 找到相对路径为 {directory}/{entryName}.* 的数据文件项
        /// </summary>
        /// <param name="modid"></param>
        /// <param name="registry"></param>
        /// <param name="directory"></param>
        /// <param name="entryName"></param>
        /// <param name="dataEntryChain"></param>
        /// <returns></returns>
        internal bool TryGetDataChain(string modid, string registry, string directory, string entryName, List<RawDataEntry> dataEntryChain);

        /// <summary>
        /// 枚举目录 {modid}/{direcotry}/ 下的所有数据文件项
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="direcotry"></param>
        /// <param name="dataEntries"></param>
        internal void EnumerateData(string registry, string direcotry, List<RawDataEntry> dataEntries);

        /// <summary>
        /// 枚举目录 {modid}/{direcotry}/ 下的所有数据文件项
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="direcotry"></param>
        /// <param name="dataEntryChains"></param>
        internal void EnumerateDataChain(string registry, string direcotry, List<List<RawDataEntry>> dataEntryChains);
    }

    public static class DataProviderExtension
    {
        public static bool TryLoadData<T>(this IDataProvider dataProvider, StringID dataRegistry, StringID id, IDataReader<T> dataReader, [NotNullWhen(true)] out T? data)
            where T : class
        {
            string modidRegistry = Path.Combine(id.namspace, dataRegistry.path);
            string directory = Path.Combine(modidRegistry, id.path);
            string entryName = id.path;
            data = null;
            return dataProvider.TryGetData(modidRegistry, directory, entryName, out RawDataEntry dataEntry) &&
                dataReader.TryReadDataFrom(dataEntry.stream, dataEntry.id, out data);
        }

        public static bool TryLoadData<T>(this IDataProvider dataProvider, StringID dataRegistry, string prefix, StringID id, IDataReader<T> dataReader, [NotNullWhen(true)] out T? data)
            where T : class
        {
            string modidRegistry = Path.Combine(id.namspace, dataRegistry.path);
            string directory = Path.Combine(modidRegistry, prefix, id.path);
            string entryName = id.path;
            data = null;
            return dataProvider.TryGetData(modidRegistry, directory, entryName, out RawDataEntry dataEntry) &&
                dataReader.TryReadDataFrom(dataEntry.stream, dataEntry.id, out data);
        }


        public static List<T> EnumrateData<T>(this IDataProvider dataProvider, StringID dataRegistry, IDataReader<T> dataReader)
            where T : class
        {
            string registry = dataRegistry.path;
            List<RawDataEntry> dataEntries = new List<RawDataEntry>();
            List<T> dataList = new List<T>();
            dataProvider.EnumerateData(registry, registry, dataEntries);
            foreach (RawDataEntry dataEntry in dataEntries)
            {
                if (dataReader.TryReadDataFrom(dataEntry.stream, dataEntry.id, out T? data))
                {
                    dataList.Add(data);
                }
            }
            return dataList;
        }

        public static List<T> EnumrateData<T>(this IDataProvider dataProvider, StringID dataRegistry, string prefix, IDataReader<T> dataReader)
            where T : class
        {
            string registry = dataRegistry.path;
            string directory = Path.Combine(registry, prefix);
            List<RawDataEntry> dataEntries = new List<RawDataEntry>();
            List<T> dataList = new List<T>();
            dataProvider.EnumerateData(registry, directory, dataEntries);
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
