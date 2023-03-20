using StarCube.Utility;
using StarCube.Core.Component;

namespace StarCube.Game.Entity.Components
{
    public abstract class TickComponent : Component<Entity>
    {
        public static readonly ComponentType<Entity, TransformComponent> COMPONENT_TYPE =
            new ComponentType<Entity, TransformComponent>(StringID.Create(Constants.DEFAULT_NAMESPACE, "tick"));

        public abstract void OnTick();
    }
}
