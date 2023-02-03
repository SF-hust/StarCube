using StarCube.Utility;
using StarCube.Resource;
using StarCube.BootStrap;
using StarCube.Core.Registry;

namespace StarCube.Game.Block
{
    [ConstructInBootStrap]
    public static class Blocks
    {
        public static Block Air = new Block().BuildSingleBlockState();

        static Blocks()
        {
            Registries.BlockRegistry.OnRegisterStartEvent +=
                (object sender, RegisterStartEventArgs args) => 
                Registries.BlockRegistry.Register(StringID.Create(Constants.DEFAULT_NAMESPACE, "air"), Air);
        }
    }
}
