using StarCube.Utility;
using StarCube.Bootstrap.Attributes;
using StarCube.Core.Registry;

namespace StarCube.Game.Block
{
    [BootstrapClass]
    public static class Blocks
    {
        public static Block Air = new Block().BuildSingleBlockState();

        static Blocks()
        {
            Registries.BLOCK.OnRegisterStartEvent +=
                (object sender, RegisterStartEventArgs args) => 
                Registries.BLOCK.Register(StringID.Create(Constants.DEFAULT_NAMESPACE, "air"), Air);
        }
    }
}
