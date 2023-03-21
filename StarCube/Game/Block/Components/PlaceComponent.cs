using StarCube.Utility;
using StarCube.Utility.Math;
using StarCube.Core.Component;
using StarCube.Core.Component.Attributes;
using StarCube.Game.Level;

namespace StarCube.Game.Block.Components
{
    [ComponentType]
    public abstract class PlaceComponent : Component<Block>
    {
        public static readonly StringID ComponentID = StringID.Create(Constants.DEFAULT_NAMESPACE, "place");

        public static readonly ComponentType<Block, RandomTickComponent> COMPONENT_TYPE =
            new ComponentType<Block, RandomTickComponent>(ComponentID);

        public abstract bool OnPlace(BlockPos pos, ILevel level, out BlockState? blockState);
    }
}
