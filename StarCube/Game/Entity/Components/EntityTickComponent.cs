using StarCube.Utility;
using StarCube.Core.Component;

namespace StarCube.Game.Entity.Components
{
    public abstract class EntityTickComponent : Component<Entity>
    {
        public static readonly ComponentType<Entity, EntityTickComponent> COMPONENT_TYPE =
            new ComponentType<Entity, EntityTickComponent>(StringID.Create(Constants.DEFAULT_NAMESPACE, "tick"));

        public abstract void OnTick();
    }
}
