using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using StarCube.Utility;
using StarCube.Data.Provider;

namespace StarCube.Data.Loading
{
    public class DataLoadingContext
    {
        public bool TryGetDataResult(StringID id, [NotNullWhen(true)] out object? result)
        {
            return idToDataLoadResult.TryGetValue(id, out result);
        }

        public bool TryGetDataResult<T>(StringID id, [NotNullWhen(true)] out T? result)
            where T : class
        {
            if(idToDataLoadResult.TryGetValue(id, out object? obj) && obj is T ret)
            {
                result = ret;
                return true;
            }

            result = null;
            return false;
        }

        public void AddDataResult(StringID id, object result, bool temp)
        {
            idToDataLoadResult.Add(id, result);
            if(temp)
            {
                tempDataResultIDs.Add(id);
            }
        }

        public void ClearTempDataResult()
        {
            foreach (StringID id in tempDataResultIDs)
            {
                idToDataLoadResult.Remove(id);
            }
            tempDataResultIDs.Clear();
        }

        public DataLoadingContext(IDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
            idToDataLoadResult = new Dictionary<StringID, object>();
            tempDataResultIDs = new HashSet<StringID>();
        }

        public readonly IDataProvider dataProvider;

        private readonly Dictionary<StringID, object> idToDataLoadResult;

        private readonly HashSet<StringID> tempDataResultIDs;
    }
}
