﻿using System;

using StarCube.Core.Registry;
using StarCube.Core.State;

namespace StarCube.Game.Block
{
    public class Block :
        IRegistryEntry<Block>,
        IStateDefiner<Block, BlockState>
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

        /*
         * 作为 StateDefiner
         */

        public StateDefinition<Block, BlockState> StateDefinition { get => stateDefinition!; set => stateDefinition ??= value; }

        private StateDefinition<Block, BlockState>? stateDefinition;

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}