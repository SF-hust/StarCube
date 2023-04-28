using StarCube.Utility.Math;
using StarCube.Core.Components;
using StarCube.Game.Entities;
using StarCube.Game.Levels;
using StarCube.Game.Items;

namespace StarCube.Game.Blocks.Components
{
    public abstract class BlockUseComponent : Component<Block>
    {
        public abstract void OnUse(Level level, BlockPos pos, Entity? user, ItemStack? itemStack);

        public BlockUseComponent(ComponentType<Block> type) : base(type)
        {
        }
    }
}
