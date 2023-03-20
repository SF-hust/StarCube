using StarCube.Utility;
using StarCube.Utility.Math;
using StarCube.Core.Component;
using StarCube.Game.Level;

namespace StarCube.Game.Block.Components
{
    public abstract class PlaceComponent : Component<Block>
    {
        public static readonly ComponentType<Block, RandomTickComponent> COMPONENT_TYPE =
            new ComponentType<Block, RandomTickComponent>(StringID.Create(Constants.DEFAULT_NAMESPACE, "place"));

        public abstract bool OnPlace(BlockPos pos, ILevel level, out BlockState? blockState);
    }
}
