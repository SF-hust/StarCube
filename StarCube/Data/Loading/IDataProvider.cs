using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using StarCube.Data.DependencyResolver;

namespace StarCube.Data.Loading
{
    public interface IDataProvider
    {
        public readonly struct DataEntry
        {
            public readonly StringID id;
            public readonly FileStream stream;

            public DataEntry(StringID id, FileStream stream)
            {
                this.id = id;
                this.stream = stream;
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

        public IEnumerable<DataEntry> EnumerateData(StringID dataRegistry)
        {
            return EnumerateData(dataRegistry, DataFilterMode.None);
        }

        public IEnumerable<DataEntry> EnumerateData(StringID dataRegistry, DataFilterMode filterMode);

        public bool TryGet(StringID dataRegistry, StringID id, [NotNullWhen(true)] out FileStream? stream);
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
    }
}
