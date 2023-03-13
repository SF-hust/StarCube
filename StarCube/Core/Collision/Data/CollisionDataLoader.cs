using System;
using System.Collections.Generic;

using StarCube.Utility;
using StarCube.Data.Loading;
using StarCube.Data.Provider;

namespace StarCube.Core.Collision.Data
{
    public class CollisionDataLoader : IDataLoader
    {
        public void Run(IDataProvider dataProvider)
        {
            List<StringID> missingDataIDs = dataProvider.LoadDataDictionary(CollisionData.DataRegistry, dataIDs, CollisionData.DataReader, out Dictionary<StringID, CollisionData> collisionDataList);

            consumeResult(collisionDataList, missingDataIDs);
        }

        public CollisionDataLoader(IEnumerable<StringID> dataIDs, Action<Dictionary<StringID, CollisionData>, List<StringID>> resultConsumer)
        {
            this.dataIDs = dataIDs;
            consumeResult = resultConsumer;
        }

        private readonly IEnumerable<StringID> dataIDs;

        private readonly Action<Dictionary<StringID, CollisionData>, List<StringID>> consumeResult;
    }
}
