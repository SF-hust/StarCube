using StarCube.Utility;
using StarCube.Core.Component;
using StarCube.Core.Component.Attributes;
using StarCube.Core.Registries;

namespace StarCube.Game.Entities.Components
{
    [ComponentType]
    public abstract class EntityTickComponent : Component<Entity>
    {
        public static readonly StringID ComponentTypeID = StringID.Create(Constants.DEFAULT_NAMESPACE, "tick");

        public static readonly ComponentType<Entity, EntityTickComponent> COMPONENT_TYPE =
            new ComponentType<Entity, EntityTickComponent>(BuiltinRegistries.ENTITY_COMPONENT_TYPE, ComponentTypeID);

        public abstract void OnTick();
    }
}
