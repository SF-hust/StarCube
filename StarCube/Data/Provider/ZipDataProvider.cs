using System;
using System.Collections.Generic;

namespace StarCube.Data.Provider
{
    public class ZipDataProvider : IDataProvider
    {
        void IDataProvider.EnumerateData(string registry, string directory, List<RawDataEntry> dataEntries)
        {
            throw new NotImplementedException();
        }

        void IDataProvider.EnumerateDataChain(string registry, string directory, List<List<RawDataEntry>> dataEntryChains)
        {
            throw new NotImplementedException();
        }

        bool IDataProvider.TryGetData(string modid, string registry, string directory, string entryName, out RawDataEntry dataEntry)
        {
            throw new NotImplementedException();
        }

        bool IDataProvider.TryGetDataChain(string modid, string registry, string directory, string entryName, List<RawDataEntry> dataEntryChain)
        {
            throw new NotImplementedException();
        }
    }
}
