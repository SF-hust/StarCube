using StarCube.Utility;
using StarCube.Core.Components;
using StarCube.Core.Components.Attributes;
using StarCube.Core.Registries;

namespace StarCube.Game.BlockEntities.Components
{
    public abstract class BlockEntityTickComponent : Component<BlockEntity>
    {
        public abstract void OnTick();

        public BlockEntityTickComponent(ComponentType<BlockEntity> type) : base(type)
        {
        }
    }
}
