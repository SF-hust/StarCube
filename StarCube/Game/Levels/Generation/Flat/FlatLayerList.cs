using System.Collections.Generic;

using StarCube.Utility.Container;
using StarCube.Game.Block;

namespace StarCube.Game.Levels.Generation.Flat
{
    public sealed class FlatLayerList
    {
        private readonly struct Layer
        {
            public Layer(int blockStateID, int start, int end)
            {
                this.blockStateID = blockStateID;
                this.start = start;
                this.end = end;
            }

            public readonly int blockStateID;
            public readonly int start;
            public readonly int end;
        }

        public FlatLayerList Set(int height, BlockState blockState)
        {
            layers.Add(new Layer(globalBlockStateIDMap.IdFor(blockState), height, height + 1));
            return this;
        }

        public FlatLayerList SetRange(int from, int to, BlockState blockState)
        {
            layers.Add(new Layer(globalBlockStateIDMap.IdFor(blockState), from, to + 1));
            return this;
        }

        public BlockState GetBlockStateForHeight(int height)
        {
            foreach (Layer slice in layers)
            {
                if(slice.start < height && slice.end > height)
                {
                    return globalBlockStateIDMap.ValueFor(slice.blockStateID);
                }
            }

            return BuiltinBlocks.Air.StateDefinition.defaultState;
        }

        public FlatLayerList(IIDMap<BlockState> globalBlockStateIDMap)
        {
            this.globalBlockStateIDMap = globalBlockStateIDMap;
        }

        private readonly IIDMap<BlockState> globalBlockStateIDMap;

        private readonly List<Layer> layers = new List<Layer>();
    }
}
