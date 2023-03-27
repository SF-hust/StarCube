using StarCube.Utility;
using StarCube.Core.Component;
using StarCube.Core.Component.Attributes;
using StarCube.Game.Level;
using StarCube.Utility.Math;

namespace StarCube.Game.Block.Components
{
    [ComponentType]
    public abstract class UseComponent : Component<Block>
    {
        public static readonly ComponentType<Block, RandomTickComponent> COMPONENT_TYPE = new ComponentType<Block, RandomTickComponent>(StringID.Create(Constants.DEFAULT_NAMESPACE, "random_tick"));

        public abstract override ComponentVariant<Block> Variant { get; }

        public abstract void OnUse(WorldLevel level, BlockPos pos, Entities.Entity user);
    }
}
