using System;

using StarCube.Core.Registry;

namespace StarCube.Game.Item
{
    public class Item : IRegistryEntry<Item>
    {
        public RegistryEntryData<Item> RegistryData
        {
            get => IRegistryEntry<Item>.RegistryEntryGetHelper(regData);
            set => IRegistryEntry<Item>.RegistryEntrySetHelper(ref regData, value);
        }
        private RegistryEntryData<Item>? regData = null;

        public Type AsEntryType => typeof(Item);
    }
}
