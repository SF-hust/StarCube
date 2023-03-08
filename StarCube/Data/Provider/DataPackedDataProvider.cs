using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace StarCube.Data.Provider
{
    public class DataPackedDataProvider : IDataProvider
    {
        public IEnumerable<IDataProvider.DataEntry> EnumerateData(StringID dataRegistry, IDataProvider.DataFilterMode filterMode)
        {
            throw new NotImplementedException();
        }

        public bool TryGet(StringID dataRegistry, StringID id, [NotNullWhen(true)] out FileStream? stream)
        {
            throw new NotImplementedException();
        }
    }
}
