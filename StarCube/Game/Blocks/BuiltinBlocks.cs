using StarCube.Utility;
using StarCube.Bootstrap.Attributes;
using StarCube.Core.Registries;
using StarCube.Game.Blocks;

[assembly: RegisterBootstrapClass(typeof(BuiltinBlocks))]
namespace StarCube.Game.Blocks
{
    public static class BuiltinBlocks
    {
        public static Block Air = new Block(StringID.Create(Constants.DEFAULT_NAMESPACE, "air"), BlockProperties.Builder.Create().Air().Build());

        static BuiltinBlocks()
        {
            BuiltinRegistries.Block.OnRegisterStartEvent +=
                (object sender, RegisterStartEventArgs args) => 
                BuiltinRegistries.Block.Register(Air);
        }
    }
}
