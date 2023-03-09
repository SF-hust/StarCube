using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace StarCube.Data.Provider
{
    public class DataPackedDataProvider : IDataProvider
    {
        void IDataProvider.EnumerateData(string registry, string direcotry, List<RawDataEntry> dataEntries)
        {
            throw new NotImplementedException();
        }

        void IDataProvider.EnumerateDataChain(string registry, string direcotry, List<List<RawDataEntry>> dataEntryChains)
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
