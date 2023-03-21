using StarCube.Utility;
using StarCube.Core.Component;
using StarCube.Core.Component.Attributes;

namespace StarCube.Game.Block.Components
{
    [ComponentType]
    public abstract class RandomTickComponent : Component<Block>
    {
        public static readonly ComponentType<Block, RandomTickComponent> COMPONENT_TYPE =
            new ComponentType<Block, RandomTickComponent>(StringID.Create(Constants.DEFAULT_NAMESPACE, "random_tick"));

        public abstract void OnRandomTick();
    }
}
