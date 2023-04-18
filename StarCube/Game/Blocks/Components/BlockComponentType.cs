using StarCube.Utility;
using StarCube.Core.Components;
using StarCube.Core.Registries;

namespace StarCube.Game.Blocks.Components
{
    public abstract class BlockComponentType<T> : ComponentType<Block, T>
        where T : Component<Block>
    {
        public BlockComponentType(StringID id) : base(BuiltinRegistries.BlockComponentType, id)
        {
        }
    }
}
