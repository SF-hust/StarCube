using StarCube.Utility;
using StarCube.Core.Components;
using StarCube.Core.Components.Attributes;
using StarCube.Core.Registries;
using StarCube.Game.Entities.Components;

namespace StarCube.Game.Entities.Components
{
    public abstract class EntityTickComponent : Component<Entity>
    {
        public static readonly StringID ComponentTypeID = StringID.Create(Constants.DEFAULT_NAMESPACE, "tick");

        public static readonly ComponentType<Entity, EntityTickComponent> COMPONENT_TYPE =
            new ComponentType<Entity, EntityTickComponent>(BuiltinRegistries.ENTITY_COMPONENT_TYPE, ComponentTypeID);

        public abstract void OnTick();
    }
}
