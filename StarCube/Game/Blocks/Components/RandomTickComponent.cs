using StarCube.Utility.Math;
using StarCube.Core.Components;
using StarCube.Game.Levels;

namespace StarCube.Game.Blocks.Components
{
    public abstract class RandomTickComponent: Component<Block>
    {
        public abstract void OnRandomTick(Level level, BlockPos pos, BlockState blockState);

        public RandomTickComponent(ComponentType<Block> type) : base(type)
        {
        }
    }
}
