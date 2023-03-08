using System;

using StarCube.Core.Registry;

namespace StarCube.Game.Item
{
    public class Item : IRegistryEntry<Item>
    {
        public RegistryEntryData<Item> RegistryEntryData
        {
            get => IRegistryEntry<Item>.RegistryEntryGetHelper(regData);
            set => IRegistryEntry<Item>.RegistryEntrySetHelper(ref regData, value);
        }

        public Type AsEntryType => typeof(Item);

        public Item()
        {
        }

        private RegistryEntryData<Item>? regData = null;
    }
}
