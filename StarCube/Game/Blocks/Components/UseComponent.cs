using StarCube.Utility;
using StarCube.Core.Component;
using StarCube.Core.Component.Attributes;
using StarCube.Game.Levels;
using StarCube.Utility.Math;

namespace StarCube.Game.Blocks.Components
{
    [ComponentType]
    public abstract class UseComponent : Component<Block>
    {
        public static readonly ComponentType<Block, RandomTickComponent> COMPONENT_TYPE = new ComponentType<Block, RandomTickComponent>(StringID.Create(Constants.DEFAULT_NAMESPACE, "random_tick"));

        public abstract override ComponentVariant<Block> Variant { get; }

        public abstract void OnUse(Level level, BlockPos pos, Entities.Entity user);
    }
}
