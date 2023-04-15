using StarCube.Utility;
using StarCube.Bootstrap.Attributes;

using StarCube.Core.Registries;
using StarCube.Game.Items;

[assembly: BootstrapClass(typeof(BuiltinItems))]
namespace StarCube.Game.Items
{
    public static class BuiltinItems
    {
        public static Item Air = new Item(StringID.Create(Constants.DEFAULT_NAMESPACE, "air"));

        static BuiltinItems()
        {
            BuiltinRegistries.ITEM.OnRegisterStartEvent +=
                (object sender, RegisterStartEventArgs args) =>
                BuiltinRegistries.ITEM.Register(Air);
        }
    }
}
