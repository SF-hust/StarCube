using System;
using System.Collections.Generic;

using StarCube.Utility;
using StarCube.Data.Loading;
using StarCube.Data.Provider;

namespace StarCube.Game.Block.Data
{
    public class BlockStateCollisionDataLoader : IDataLoader
    {
        public void Run(IDataProvider dataProvider)
        {
            List<BlockStateCollisionData> dataList = new List<BlockStateCollisionData>();
            List<StringID> missingDataIDs = new List<StringID>();

            foreach(Block block in blocks)
            {
                if(dataProvider.TryLoadData(BlockStateCollisionData.DataRegistry, block.RegistryEntryData.ID, BlockStateCollisionData.DataReader, out BlockStateCollisionData? data))
                {
                    dataList.Add(data);
                }
                else
                {
                    missingDataIDs.Add(block.RegistryEntryData.ID);
                }
            }

            consumeResult(dataList, missingDataIDs);
        }

        public BlockStateCollisionDataLoader(IEnumerable<Block> blocks, Action<List<BlockStateCollisionData>, List<StringID>> resultConsumer)
        {
            this.blocks = blocks;
            consumeResult = resultConsumer;
        }

        private readonly IEnumerable<Block> blocks;

        private readonly Action<List<BlockStateCollisionData>, List<StringID>> consumeResult;
    }
}
