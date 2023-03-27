using StarCube.Utility;
using StarCube.Core.Component;
using StarCube.Core.Component.Attributes;

namespace StarCube.Game.BlockEntity.Components
{
    [ComponentType]
    public abstract class BlockEntityTickComponent :
        Component<BlockEntity>
    {
        public static readonly ComponentType<BlockEntity, BlockEntityTickComponent> COMPONENT_TYPE =
            new ComponentType<BlockEntity, BlockEntityTickComponent>(StringID.Create(Constants.DEFAULT_NAMESPACE, "tick"));

        public abstract void OnTick();
    }
}
