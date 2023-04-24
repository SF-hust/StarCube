using System;

using StarCube.Utility;
using StarCube.Core.Registries;

namespace StarCube.Game.Entities
{
    public class EntityType : RegistryEntry<EntityType>
    {
        public Entity CreateNewEntity()
        {
            return new Entity(this, Guid.NewGuid(), true);
        }

        public EntityType(StringID id)
            : base(BuiltinRegistries.EntityType, id)
        {
        }
    }
}
