using StarCube.Utility;
using StarCube.Core.Component;

namespace StarCube.Game.BlockEntity.Components
{
    public abstract class BlockEntityTickComponent :
        Component<BlockEntity>
    {
        public static readonly ComponentType<BlockEntity, BlockEntityTickComponent> COMPONENT_TYPE =
            new ComponentType<BlockEntity, BlockEntityTickComponent>(StringID.Create(Constants.DEFAULT_NAMESPACE, "tick"));

        public abstract void OnTick();
    }
}
