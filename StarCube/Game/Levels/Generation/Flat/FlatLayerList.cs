using System.Collections.Generic;

using StarCube.Utility.Container;
using StarCube.Game.Blocks;

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

        public int GetBlockStateIDForHeight(int height)
        {
            foreach (Layer layer in layers)
            {
                if(layer.start <= height && layer.end > height)
                {
                    return layer.blockStateID;
                }
            }

            return 0;
        }

        public FlatLayerList(IIDMap<BlockState> globalBlockStateIDMap)
        {
            this.globalBlockStateIDMap = globalBlockStateIDMap;
        }

        private readonly IIDMap<BlockState> globalBlockStateIDMap;

        private readonly List<Layer> layers = new List<Layer>();
    }
}
