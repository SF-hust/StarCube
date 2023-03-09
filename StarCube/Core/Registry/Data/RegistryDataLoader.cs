using Newtonsoft.Json.Linq;

using StarCube.Data;
using StarCube.Data.Loading;
using StarCube.Data.Provider;

namespace StarCube.Core.Registry.Data
{
    public class RegistryDataLoader<T> : IDataLoader
        where T : class, IRegistryEntry<T>, new()
    {
        public void Run(IDataProvider dataProvider)
        {
            IDataReader<RegistryData> dataReader = new DataReaderWrapper<RegistryData, JObject>(RawDataReaders.JSON, RegistryData.TryParseFromJson);
            StringID dataID = StringID.Create(modid, registry.Name);

            if(!dataProvider.TryLoad(RegistryData.DataRegistry, dataID, dataReader, out RegistryData? registryData))
            {
                return;
            }

            DeferredRegister<T> deferredRegister = new DeferredRegister<T>(modid, registry);
            foreach(string entry in registryData.entries)
            {
                deferredRegister.Register(entry, new T());
            }
        }

        public RegistryDataLoader(string modid, Registry<T> registry)
        {
            this.modid = modid;
            this.registry = registry;
        }

        private readonly string modid;
        private readonly Registry<T> registry;
    }
}
