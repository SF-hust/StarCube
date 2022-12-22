using System;

using StarCube.Core.Registry;

namespace StarCube.Game.Block
{
    public class Block : IRegistryEntry<Block>
    {
        public Block()
        {

        }

        /*
         * 作为 RegistryEntry
         */

        public RegistryEntryData<Block> RegistryData { get => regData!; set => regData ??= value; }
        private RegistryEntryData<Block>? regData;

        public virtual Type AsEntryType => typeof(Block);
    }
}
