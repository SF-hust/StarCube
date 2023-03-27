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
            Registries.ITEM.OnRegisterStartEvent +=
                (object sender, RegisterStartEventArgs args) =>
                Registries.ITEM.Register(StringID.Create(Constants.DEFAULT_NAMESPACE, "air"), Air);
        }
    }
}
