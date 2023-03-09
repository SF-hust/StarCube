using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using StarCube.Data.Loading;
using StarCube.Data.DependencyResolver;
using StarCube.Data.Provider.DataSource;
using System;
using StarCube.Utility;

namespace StarCube.Data.Provider
{
    public interface IDataProvider
    {
        public readonly struct RawDataEntry
        {
            public readonly StringID id;
            public readonly FileStream stream;
            public readonly IDataSource source;

            public RawDataEntry(StringID id, FileStream stream, IDataSource source)
            {
                this.id = id;
                this.stream = stream;
                this.source = source;
            }
        }

        public class DataFilterMode
        {
            public static readonly DataFilterMode None = new DataFilterMode(string.Empty);

            public readonly string directoryPrefix;

            public DataFilterMode(string directoryPrefix)
            {
                this.directoryPrefix = directoryPrefix;
            }
        }

        public bool TryGet(StringID dataRegistry, StringID id, [NotNullWhen(true)] out FileStream? stream);

        public bool TryGetDataChain(StringID dataRegistry, StringID id, [NotNullWhen(true)] out List<RawDataEntry> dataEntries)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<List<RawDataEntry>> EnumerateDataChains(StringID dataRegistry, DataFilterMode filterMode)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<RawDataEntry> EnumerateData(StringID dataRegistry)
        {
            return EnumerateData(dataRegistry, DataFilterMode.None);
        }

        public IEnumerable<RawDataEntry> EnumerateData(StringID dataRegistry, DataFilterMode filterMode);
    }

    public static class DataProviderExtension
    {
        public static bool TryLoad<T>(this IDataProvider dataProvider, StringID dataRegistry, StringID id, IDataReader<T> dataReader, [NotNullWhen(true)] out T? data)
            where T : class
        {
            data = null;
            return dataProvider.TryGet(dataRegistry, id, out FileStream? stream) && dataReader.TryReadDataFrom(stream, id, out  data);
        }

        public static Dictionary<StringID, T> LoadDataWithDependencies<T>(this IDataProvider dataProvider, StringID dataRegistry, IEnumerable<StringID> ids, IDataReader<T> dataReader)
            where T : class, IUnresolvedData<T>
        {
            Dictionary<StringID, T> idToData = new Dictionary<StringID, T>();

            foreach (StringID id in ids)
            {
                void LoadWithDependency(StringID dataID)
                {
                    if(!dataProvider.TryLoad(dataRegistry, dataID, dataReader, out T? data))
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

        public static IEnumerable<T> EnumerateData<T>(this IDataProvider dataProvider, StringID dataRegistry, IDataProvider.DataFilterMode filterMode, IDataReader<T> dataReader)
            where T : class, IStringID
        {
            List<T> dataList = new List<T>();
            foreach (IDataProvider.RawDataEntry entry in dataProvider.EnumerateData(dataRegistry, filterMode))
            {
                if(dataReader.TryReadDataFrom(entry.stream, entry.id, out T? data))
                {
                    dataList.Add(data);
                }
            }
            return dataList;
        }
    }
}
