using System;

using StarCube.Core.Registries;
using StarCube.Utility;

namespace StarCube.Game.Entities
{
    public class EntityType : RegistryEntry<EntityType>
    {
        public Entity CreateNewEntity()
        {
            return new Entity(this, Guid.NewGuid());
        }

        public EntityType(StringID id)
            : base(BuiltinRegistries.ENTITY_TYPE, id)
        {
        }
    }
}
