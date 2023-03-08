using System;

using StarCube.Core.Registry;

namespace StarCube.Game.Entity
{
    public class EntityType : IRegistryEntry<EntityType>
    {
        public RegistryEntryData<EntityType> RegistryEntryData
        {
            get => IRegistryEntry<EntityType>.RegistryEntryGetHelper(regData);
            set => IRegistryEntry<EntityType>.RegistryEntrySetHelper(ref regData, value);
        }

        public Type AsEntryType => typeof(EntityType);

        public EntityType()
        {
        }

        private RegistryEntryData<EntityType>? regData = null;
    }
}
