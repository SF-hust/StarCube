using System.Collections.Generic;
using System.Text;
using System.Linq;

using StarCube.Utility;
using StarCube.Utility.Logging;
using StarCube.Data.Loading;
using StarCube.Data.Loading.Attributes;
using StarCube.Data.Provider;
using StarCube.Core.Registries;
using StarCube.Core.Registries.Data;
using StarCube.Game.Blocks.Data;

[assembly: RegisterDataLoader(typeof(BlockStateCollisionDataLoader))]
namespace StarCube.Game.Blocks.Data
{
    public class BlockStateCollisionDataLoader : DataLoader
    {
        public static readonly StringID ID = BlockStateCollisionData.DataRegistry;

        public override void Run(DataLoadingContext context)
        {
            IEnumerable<StringID> blockIDs = from block in BuiltinRegistries.Block select block.ID;

            List<StringID> missingDataIDs = context.dataProvider.LoadDataDictionary(
                BlockStateCollisionData.DataRegistry,
                blockIDs, BlockStateCollisionData.DataReader,
                out Dictionary<StringID, BlockStateCollisionData>? idToBlockStateCollisionData);

            foreach (StringID id in missingDataIDs)
            {
                StringBuilder builder = StringUtil.StringBuilder;
                builder.Append("missing blockstate collision data : \"").Append(id).Append("\"");
                LogUtil.Warning(builder.ToString());
            }

            context.AddDataResult(ID, idToBlockStateCollisionData);
        }

        public BlockStateCollisionDataLoader()
            : base(ID, false)
        {
            dependencies.Add(BlockRegistryDataLoader.ID);
            dependencies.Add(RegistryLoader.ID);
        }
    }
}
