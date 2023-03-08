using StarCube.Utility;
using StarCube.BootStrap.Attributes;
using StarCube.Data;
using StarCube.Core.Registry;

namespace StarCube.Game.Item
{
    [ConstructInBootStrap]
    public class Items
    {
        public static Item Air = new Item();

        static Items()
        {
            Registries.BLOCK.OnRegisterStartEvent +=
                (object sender, RegisterStartEventArgs args) =>
                Registries.BLOCK.Register(StringID.Create(Constants.DEFAULT_NAMESPACE, "air"), Air);
        }
    }
}
