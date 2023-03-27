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

        public ComponentContainer<BlockEntity> Components => componentHolder;

        public BlockEntity(BlockEntityType type, Guid guid)
        {
            this.type = type;
            this.guid = guid;
            componentHolder = new ComponentContainer<BlockEntity>(this);
        }

        public readonly BlockEntityType type;

        public readonly Guid guid;

        private readonly ComponentContainer<BlockEntity> componentHolder;
    }
}
