using StarCube.Utility;
using StarCube.Core.Components;
using StarCube.Core.Registries;

namespace StarCube.Game.BlockEntities.Components
{
    public abstract class BlockEntityComponentType<T> : ComponentType<BlockEntity, T>
        where T : Component<BlockEntity>
    {
        public BlockEntityComponentType(StringID id) : base(BuiltinRegistries.BlockEntityComponentType, id)
        {
        }
    }
}
