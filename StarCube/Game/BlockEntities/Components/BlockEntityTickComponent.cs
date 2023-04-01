using StarCube.Utility;
using StarCube.Core.Component;
using StarCube.Core.Component.Attributes;
using StarCube.Core.Registries;

namespace StarCube.Game.BlockEntities.Components
{
    [ComponentType]
    public abstract class BlockEntityTickComponent :
        Component<BlockEntity>
    {
        public static readonly StringID ComponentTypeID = StringID.Create(Constants.DEFAULT_NAMESPACE, "tick");

        public static readonly ComponentType<BlockEntity, BlockEntityTickComponent> COMPONENT_TYPE =
            new ComponentType<BlockEntity, BlockEntityTickComponent>(BuiltinRegistries.BLOCK_ENTITY_COMPONENT_TYPE, ComponentTypeID);

        public abstract void OnTick();
    }
}
