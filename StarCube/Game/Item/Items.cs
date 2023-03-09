using StarCube.Utility;
using StarCube.BootStrap.Attributes;
using StarCube.Core.Registry;

namespace StarCube.Game.Item
{
    [ConstructInBootStrap]
    public class Items
    {
        public static readonly Item Air = new Item();

        static Items()
        {
            Registries.ITEM.OnRegisterStartEvent +=
                (object sender, RegisterStartEventArgs args) =>
                Registries.ITEM.Register(StringID.Create(Constants.DEFAULT_NAMESPACE, "air"), Air);
        }
    }
}
