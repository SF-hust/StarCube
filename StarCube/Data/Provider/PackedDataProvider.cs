using System;
using System.Collections.Generic;

namespace StarCube.Data.Provider
{
    public class PackedDataProvider : IDataProvider
    {
        /* ~ IDataProvider 接口实现 end ~ */
        bool IDataProvider.TryGetData(string modid, string registry, string path, out RawDataEntry dataEntry)
        {
            throw new NotImplementedException();
        }

        void IDataProvider.EnumerateData(string registry, string direcotry, List<RawDataEntry> dataEntries)
        {
            throw new NotImplementedException();
        }
        /* ~ IDataProvider 接口实现 end ~ */
    }
}
