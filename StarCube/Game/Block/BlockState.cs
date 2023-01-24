using StarCube.Core.State;

namespace StarCube.Game.Block
{
    public class BlockState : StateHolder<Block, BlockState>
    {
        public static BlockState Create(Block block, StatePropertyList properties)
        {
            return new BlockState(block, properties);
        }

        public Block Block { get => owner; }

        public BlockState(Block block, StatePropertyList properties)
            : base(block, properties)
        {
        }
    }
}
