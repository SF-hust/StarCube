using System;

using StarCube.Utility;
using StarCube.Core.Components;

namespace StarCube.Game.BlockEntities
{
    public class BlockEntity :
        IComponentOwner<BlockEntity>,
        IGuid
    {
        Guid IGuid.Guid => guid;

        public ComponentContainer<BlockEntity> Components => componentHolder;

        public void OnActive(bool active)
        {

        }

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
