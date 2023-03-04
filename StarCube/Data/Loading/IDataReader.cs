using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace StarCube.Data.Loading
{
    public interface IDataReader<T>
        where T : class
    {
        public bool TryLoadData(IDataProvider dataProvider, StringID id, [NotNullWhen(true)] out T? data);
    }
}
