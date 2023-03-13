using System;
using System.Collections.Generic;
using System.Linq;

using StarCube.Utility;
using StarCube.Data.Loading;
using StarCube.Data.Provider;

namespace StarCube.Game.Block.Data
{
    public class BlockStateCollisionDataLoader : IDataLoader
    {
        public void Run(IDataProvider dataProvider)
        {
            IEnumerable<StringID> blockIDs = from block in blocks select block.RegistryEntryData.ID;

            List<StringID> missingDataIDs = dataProvider.LoadDataDictionary(
                BlockStateCollisionData.DataRegistry,
                blockIDs, BlockStateCollisionData.DataReader,
                out Dictionary<StringID, BlockStateCollisionData>? dataDictionary);

            consumeResult(dataDictionary, missingDataIDs);
        }

        public BlockStateCollisionDataLoader(IEnumerable<Block> blocks, Action<Dictionary<StringID, BlockStateCollisionData>, List<StringID>> resultConsumer)
        {
            this.blocks = blocks;
            consumeResult = resultConsumer;
        }

        private readonly IEnumerable<Block> blocks;

        private readonly Action<Dictionary<StringID, BlockStateCollisionData>, List<StringID>> consumeResult;
    }
}
