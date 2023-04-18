using StarCube.Utility;
using StarCube.Bootstrap.Attributes;

using StarCube.Core.Registries;
using StarCube.Game.Items;

[assembly: RegisterBootstrapClass(typeof(BuiltinItems))]
namespace StarCube.Game.Items
{
    public static class BuiltinItems
    {
        public static Item Air = new Item(StringID.Create(Constants.DEFAULT_NAMESPACE, "air"));

        static BuiltinItems()
        {
            BuiltinRegistries.Item.OnRegisterStartEvent +=
                (object sender, RegisterStartEventArgs args) =>
                BuiltinRegistries.Item.Register(Air);
        }
    }
}
