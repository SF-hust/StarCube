using System;
using System.Collections.Generic;

using StarCube.Utility;
using StarCube.Data.Loading;
using StarCube.Data.Provider;
using StarCube.Data.DependencyResolver;

namespace StarCube.Core.Collision.Data
{
    public class CollisionDataLoader : IDataLoader
    {
        public void Run(IDataProvider dataProvider)
        {
            List<RawCollisionData> rawCollisionData = dataProvider.LoadDataWithDependency(RawCollisionData.DataRegistry, dataIDs, RawCollisionData.DataReader);
            DependencyDataResolver<RawCollisionData, CollisionData> resolver =
                new DependencyDataResolver<RawCollisionData, CollisionData>(rawCollisionData, new CollisionDataBuilder());
            if(!resolver.TryBuildResolvedData(out Dictionary<StringID, CollisionData>? collisionData))
            {
                throw new Exception("collision data resolve failed");
            }

            consumeData(collisionData);
        }

        public CollisionDataLoader(IEnumerable<StringID> dataIDs, Action<Dictionary<StringID, CollisionData>> dataConsumer)
        {
            this.dataIDs = dataIDs;
            consumeData = dataConsumer;
        }

        private readonly IEnumerable<StringID> dataIDs;

        private readonly Action<Dictionary<StringID, CollisionData>> consumeData;
    }
}
