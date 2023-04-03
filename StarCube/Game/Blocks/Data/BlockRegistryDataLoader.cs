using System.Collections.Generic;

using StarCube.Utility;
using StarCube.Data.Loading;
using StarCube.Data.Provider;
using StarCube.Core.Registries;

namespace StarCube.Game.Blocks.Data
{
    public class BlockRegistryDataLoader : DataLoader
    {
        public readonly static StringID LoaderID = BlockRegistryData.DataRegistry;

        public BlockRegistryDataLoader() : base(LoaderID)
        {
        }

        public override void Run(DataLoadingContext context)
        {
            List<BlockRegistryData> registryDataList = context.dataProvider.EnumerateData(BlockRegistryData.DataRegistry, BlockRegistryData.DataReader);

            DeferredRegister<Block> deferredRegister = new DeferredRegister<Block>(BuiltinRegistries.BLOCK);
            foreach (BlockRegistryData data in registryDataList)
            {
                foreach (BlockRegistryDataEntry entry in data.entries)
                {
                    Block block = new Block(StringID.Create(data.id.ModidString, entry.name), entry.properties, entry.stateDefinition);
                    deferredRegister.Register(block);
                }
            }
        }
    }
}
