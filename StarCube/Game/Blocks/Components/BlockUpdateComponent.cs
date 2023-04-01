using StarCube.Utility.Math;
using StarCube.Core.Component;
using StarCube.Core.Component.Attributes;
using StarCube.Game.Levels;

namespace StarCube.Game.Blocks.Components
{
    [ComponentType]
    public abstract class BlockUpdateComponent : Component<Block>
    {
        public abstract bool OnUpdate(Level level, BlockPos blockPos, BlockState blockState);
    }
}
