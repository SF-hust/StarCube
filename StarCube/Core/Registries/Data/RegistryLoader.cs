using StarCube.Utility;
using StarCube.Core.Registries.Data;
using StarCube.Data.Loading;
using StarCube.Data.Loading.Attributes;

[assembly: RegisterDataLoader(typeof(RegistryLoader))]
namespace StarCube.Core.Registries.Data
{
    public class RegistryLoader : DataLoader
    {
        public static readonly StringID ID = StringID.Create(Constants.DEFAULT_NAMESPACE, "registry");

        public override void Run(DataLoadingContext context)
        {
            BuiltinRegistries.Root.FireRegistryEvents();
        }

        public RegistryLoader() : base(ID, false)
        {
        }
    }
}
