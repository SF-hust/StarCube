using System;
using System.Collections.Generic;

using StarCube.Utility.Container;
using StarCube.Data.Loading;
using StarCube.Data.Provider;

namespace StarCube.Game.Block.Data
{
    public class BlockStateCollisionDataLoader : IDataLoader
    {
        public void Run(IDataProvider dataProvider)
        {
            List<BlockStateCollisionData> dataList = new List<BlockStateCollisionData>();

            foreach(Block block in blocks)
            {
                if(!dataProvider.TryLoadData(BlockStateCollisionData.DataRegistry, block.RegistryEntryData.ID, BlockStateCollisionData.DataReader, out BlockStateCollisionData? data))
                {
                    continue;
                }

                dataList.Add(data);
            }
        }

        public BlockStateCollisionDataLoader(IIdMap<Block> blocks, Action<List<BlockStateCollisionData>> consumeData)
        {
            this.blocks = blocks;
            this.consumeData = consumeData;
        }

        private readonly IIdMap<Block> blocks;

        private readonly Action<List<BlockStateCollisionData>> consumeData;
    }
}
