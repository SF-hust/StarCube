using System;

using StarCube.Utility;
using StarCube.Core.Component;

namespace StarCube.Game.BlockEntity
{
    public class BlockEntity :
        IComponentHolder<BlockEntity>,
        IGuid
    {
        Guid IGuid.Guid => guid;

        public ComponentHolder<BlockEntity> Components => throw new NotImplementedException();

        public BlockEntity(BlockEntityType type, Guid guid)
        {
            this.type = type;
            this.guid = guid;
        }

        public readonly BlockEntityType type;

        public readonly Guid guid;
    }
}
