using System;

using StarCube.Core.Registry;

namespace StarCube.Game.Entities
{
    public class EntityType : IRegistryEntry<EntityType>
    {
        public RegistryEntryData<EntityType> RegistryEntryData
        {
            get => IRegistryEntry<EntityType>.RegistryEntryGetHelper(regData);
            set => IRegistryEntry<EntityType>.RegistryEntrySetHelper(ref regData, value);
        }

        public Type AsEntryType => typeof(EntityType);

        public Entity CreateNewEntity()
        {
            return new Entity(this, Guid.NewGuid());
        }

        public EntityType()
        {
        }

        private RegistryEntryData<EntityType>? regData = null;
    }
}
