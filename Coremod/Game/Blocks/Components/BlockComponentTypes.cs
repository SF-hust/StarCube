using StarCube.Bootstrap.Attributes;
using StarCube.Core.Components;
using StarCube.Core.Registries;
using StarCube.Game.Blocks;
using StarCube.Coremod.Game.Blocks.Components;

[assembly: RegisterBootstrapClass(typeof(BlockComponentTypes))]
namespace StarCube.Coremod.Game.Blocks.Components
{
    public static class BlockComponentTypes
    {
        public static ComponentType<Block> FenceUpdate = new FenceBlockUpdateComponent.Type();

        static BlockComponentTypes()
        {
            DeferredRegister<ComponentType<Block>> deferredRegister = new DeferredRegister<ComponentType<Block>>(BuiltinRegistries.BlockComponentType);
            deferredRegister.Register(FenceUpdate);
        }
    }
}
