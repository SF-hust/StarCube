using System;

using StarCube.Utility;
using StarCube.Core.Registries;

namespace StarCube.Game.Items
{
    public class Item : RegistryEntry<Item>
    {
        public Item(StringID id)
            : base(BuiltinRegistries.Item, id)
        {
        }
    }
}
