using StarCube.Core.State;
using StarCube.Core.State.Property;

namespace StarCube.Game.Blocks
{
    public static class BlockBuildHelper
    {
        public static Block BuildSingleBlockState(this Block block)
        {
            block.StateDefinition = StateDefinition<Block, BlockState>.BuildSingle(block, BlockState.Create);
            return block;
        }

        public static BlockStateBuildHelper StartBuildBlockState(this Block block)
        {
            return new BlockStateBuildHelper(block);
        }
    }

    public class BlockStateBuildHelper
    {
        public readonly Block block;

        private readonly StateDefinition<Block, BlockState>.Builder builder;

        public BlockStateBuildHelper(Block block)
        {
            this.block = block;
            builder = StateDefinition<Block, BlockState>.Builder.Create(block, BlockState.Create);
        }

        public BlockStateBuildHelper Add(string name, StateProperty property, int index)
        {
            builder.AddPropertyAndDefaultIndex(name, property, index);
            return this;
        }

        public Block Build()
        {
            block.StateDefinition = builder.Build();
            return block;
        }
    }
}
