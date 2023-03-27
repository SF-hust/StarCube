using System;

using StarCube.Core.Registry;

namespace StarCube.Game.BlockEntities
{
    public class BlockEntityType : IRegistryEntry<BlockEntityType>
    {
        public RegistryEntryData<BlockEntityType> RegistryEntryData
        {
            get => IRegistryEntry<BlockEntityType>.RegistryEntryGetHelper(regData);
            set => IRegistryEntry<BlockEntityType>.RegistryEntrySetHelper(ref regData, value);
        }

        public Type AsEntryType => typeof(BlockEntityType);

        public BlockEntity CreateNewBlockEntity()
        {
            BlockEntity blockEntity = new BlockEntity(this, Guid.NewGuid());

            return blockEntity;
        }

        public BlockEntityType()
        {
        }

        private RegistryEntryData<BlockEntityType>? regData = null;
    }
}
