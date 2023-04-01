using StarCube.Utility;
using StarCube.Bootstrap.Attributes;

using StarCube.Core.Registry;

namespace StarCube.Game.Items
{
    [BootstrapClass]
    public class BuiltinItems
    {
        public static Item Air = new Item();

        static BuiltinItems()
        {
            BuiltinRegistries.ITEM.OnRegisterStartEvent +=
                (object sender, RegisterStartEventArgs args) =>
                BuiltinRegistries.ITEM.Register(StringID.Create(Constants.DEFAULT_NAMESPACE, "air"), Air);
        }
    }
}
