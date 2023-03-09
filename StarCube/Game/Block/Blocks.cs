using StarCube.Utility;
using StarCube.BootStrap.Attributes;
using StarCube.Core.Registry;

namespace StarCube.Game.Block
{
    [ConstructInBootStrap]
    public static class Blocks
    {
        public static readonly Block Air = new Block().BuildSingleBlockState();

        static Blocks()
        {
            Registries.BLOCK.OnRegisterStartEvent +=
                (object sender, RegisterStartEventArgs args) => 
                Registries.BLOCK.Register(StringID.Create(Constants.DEFAULT_NAMESPACE, "air"), Air);
        }
    }
}
