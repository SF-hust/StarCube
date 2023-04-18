using StarCube.Utility;
using StarCube.Core.Registries;
using StarCube.Core.Components;

namespace StarCube.Game.Entities.Components
{
    public abstract class EntityComponentType<T> : ComponentType<Entity, T>
        where T : Component<Entity>
    {
        protected EntityComponentType(StringID id) : base(BuiltinRegistries.EntityComponentType, id)
        {
        }
    }
}
