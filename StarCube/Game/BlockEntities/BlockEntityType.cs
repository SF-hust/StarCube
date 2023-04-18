using System;

using StarCube.Utility;
using StarCube.Core.Registries;

namespace StarCube.Game.BlockEntities
{
    public sealed class BlockEntityType : RegistryEntry<BlockEntityType>
    {
        public BlockEntity CreateNewBlockEntity()
        {
            BlockEntity blockEntity = new BlockEntity(this, Guid.NewGuid());

            return blockEntity;
        }

        public BlockEntityType(StringID id)
            : base(BuiltinRegistries.BlockEntityType, id)
        {

        }
    }
}
