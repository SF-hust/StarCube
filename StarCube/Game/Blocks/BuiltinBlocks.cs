using StarCube.Utility;
using StarCube.Bootstrap.Attributes;
using StarCube.Core.Registries;

namespace StarCube.Game.Blocks
{
    [BootstrapClass]
    public static class BuiltinBlocks
    {
        public static Block Air = new Block(StringID.Create(Constants.DEFAULT_NAMESPACE, "air"), BlockProperties.Builder.Create().Air().Build());

        static BuiltinBlocks()
        {
            BuiltinRegistries.BLOCK.OnRegisterStartEvent +=
                (object sender, RegisterStartEventArgs args) => 
                BuiltinRegistries.BLOCK.Register(Air);
        }
    }
}
