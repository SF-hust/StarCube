using StarCube.Utility;

namespace StarCube.Game.Blocks.Data
{
    public abstract class BlockStateDataBase : IStringID
    {
        StringID IStringID.ID => id;

        public BlockStateDataBase(StringID id)
        {
            this.id = id;
        }

        /// <summary>
        /// 数据的 id，也是对应方块的 id
        /// </summary>
        public readonly StringID id;
    }
}
