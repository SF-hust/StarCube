using StarCube.Utility;
using StarCube.Utility.Math;
using StarCube.Core.Components;
using StarCube.Core.Components.Attributes;
using StarCube.Game.Levels;
using StarCube.Core.Registries;

namespace StarCube.Game.Blocks.Components
{
    [ComponentType]
    public abstract class RandomTickComponent : Component<Block>
    {
        public static readonly StringID ComponentTypeID = StringID.Create(Constants.DEFAULT_NAMESPACE, "random_tick");

        public static readonly ComponentType<Block, RandomTickComponent> COMPONENT_TYPE =
            new ComponentType<Block, RandomTickComponent>(BuiltinRegistries.BLOCK_COMPONENT_TYPE, ComponentTypeID);

        public abstract void OnRandomTick(Level level, BlockPos pos);
    }
}
