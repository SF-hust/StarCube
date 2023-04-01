using StarCube.Utility;
using StarCube.Bootstrap.Attributes;

using StarCube.Core.Registries;

namespace StarCube.Game.Items
{
    [BootstrapClass]
    public class BuiltinItems
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
