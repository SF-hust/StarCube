using System;
using System.Collections.Generic;
using System.Text;

using StarCube.Utility;
using StarCube.Utility.Logging;
using StarCube.Data.Loading;
using StarCube.Data.Loading.Attributes;
using StarCube.Data.Provider;
using StarCube.Core.Collision.Data;
using StarCube.Game.Blocks.Data;
using StarCube.Game.Blocks.Collision.Data;

[assembly: RegisterDataLoader(typeof(BlockCollisionDataLoader))]
namespace StarCube.Game.Blocks.Collision.Data
{
    public class BlockCollisionDataLoader : DataLoader
    {
        public static readonly StringID ID = StringID.Create(Constants.DEFAULT_NAMESPACE, "collision/block");

        public override void Run(DataLoadingContext context)
        {
            if (!context.TryGetDataResult(BlockStateCollisionData.DataRegistry, out Dictionary<StringID, BlockStateCollisionData>? idToBlockStateCollisionData))
            {
                throw new InvalidOperationException();
            }

            HashSet<StringID> BlockCollisionDataIDs = new HashSet<StringID>();
            foreach (BlockStateCollisionData data in idToBlockStateCollisionData.Values)
            {
                data.GetCollisionDataReferences(BlockCollisionDataIDs);
            }

            List<StringID> missingDataIDs = context.dataProvider.LoadDataDictionary(
                CollisionData.DataRegistry,
                BlockCollisionDataIDs,
                CollisionData.DataReader,
                out Dictionary<StringID, CollisionData> idToBlockCollisionData);

            foreach (StringID id in missingDataIDs)
            {
                StringBuilder builder = StringUtil.StringBuilder;
                builder.Append("missing block collision data : \"").Append(id).Append("\"");
                LogUtil.Warning(builder.ToString());
            }

            context.AddDataResult(ID, idToBlockCollisionData);
        }

        public BlockCollisionDataLoader() : base(ID, false)
        {
            dependencies.Add(BlockStateCollisionDataLoader.ID);
        }
    }
}
