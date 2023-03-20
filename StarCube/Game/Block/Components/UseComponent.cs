using StarCube.Utility;
using StarCube.BootStrap.Attributes;
using StarCube.Core.Component;

namespace StarCube.Game.Block.Components
{
    [ConstructInBootStrap]
    public abstract class UseComponent : Component<Block>
    {
        public static readonly ComponentType<Block, RandomTickComponent> COMPONENT_TYPE = new ComponentType<Block, RandomTickComponent>(StringID.Create(Constants.DEFAULT_NAMESPACE, "random_tick"));

        public abstract override ComponentVariant Variant { get; }

        public abstract void OnUse();
    }
}
