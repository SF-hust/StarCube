using StarCube.Utility.Math;
using StarCube.Core.Components;
using StarCube.Core.Components.Attributes;
using StarCube.Game.Levels;

namespace StarCube.Game.Blocks.Components
{
    [ComponentType]
    public abstract class BlockUpdateComponent : Component<Block>
    {
        public abstract bool OnUpdate(Level level, BlockPos blockPos, BlockState blockState);
    }
}
