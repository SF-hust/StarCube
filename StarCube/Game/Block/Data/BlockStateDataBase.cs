using StarCube.Utility;

namespace StarCube.Game.Block.Data
{
    public abstract class BlockStateDataBase : IStringID
    {
        StringID IStringID.ID => id;

        public BlockStateDataBase(StringID id)
        {
            this.id = id;
        }

        public readonly StringID id;
    }
}
