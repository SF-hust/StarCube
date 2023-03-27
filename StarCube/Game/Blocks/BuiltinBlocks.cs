using StarCube.Utility;
using StarCube.Bootstrap.Attributes;
using StarCube.Core.Registry;

namespace StarCube.Game.Blocks
{
    [BootstrapClass]
    public static class BuiltinBlocks
    {
        public static Block Air = new Block().BuildSingleBlockState();

        static BuiltinBlocks()
        {
            Registries.BLOCK.OnRegisterStartEvent +=
                (object sender, RegisterStartEventArgs args) => 
                Registries.BLOCK.Register(StringID.Create(Constants.DEFAULT_NAMESPACE, "air"), Air);
        }
    }
}
