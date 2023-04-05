using StarCube.Utility;
using StarCube.Data.Loading;
using StarCube.Data.Loading.Attributes;
using StarCube.Core.Registries.Data;
using StarCube.Game.Blocks.Data;

[assembly: RegisterDataLoader(typeof(BlockStateLoader))]
namespace StarCube.Game.Blocks.Data
{
    public class BlockStateLoader : DataLoader
    {
        public static readonly StringID ID = StringID.Create(Constants.DEFAULT_NAMESPACE, "blockstate");

        public override void Run(DataLoadingContext context)
        {
            BlockState.BuildGlobalBlockStateIDMap();
        }

        public BlockStateLoader()
            : base(ID, false)
        {
            dependencies.Add(BlockRegistryDataLoader.ID);
            dependencies.Add(RegistryLoader.ID);
        }
    }
}
