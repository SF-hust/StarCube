using StarCube.Core.Components;

namespace StarCube.Game.Entities.Components
{
    public abstract class EntityTickComponent : Component<Entity>
    {
        public abstract void OnTick();

        public EntityTickComponent(ComponentType<Entity> type) : base(type)
        {
        }
    }
}
