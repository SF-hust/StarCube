﻿using System;

using StarCube.Core.Registry;
using StarCube.Core.State;
using StarCube.Utility;

namespace StarCube.Game.Block
{
    public class Block :
        IRegistryEntry<Block>,
        IStateDefiner<Block, BlockState>
    {
        public Block()
        {
        }

        /* ~ IRegistryEntry<Block> 接口实现 start ~ */
        public RegistryEntryData<Block> RegistryEntryData
        {
            get => IRegistryEntry<Block>.RegistryEntryGetHelper(regData);
            set => IRegistryEntry<Block>.RegistryEntrySetHelper(ref regData, value);
        }
        private RegistryEntryData<Block>? regData = null;

        public virtual Type AsEntryType => typeof(Block);
        public Registry Registry => regData!.registry;
        public StringID ID => regData!.id;
        public int IntegerID => regData!.integerID;
        public string Modid => regData!.Modid;
        public string Name => regData!.Name;
        /* ~ IRegistryEntry<Block> 接口实现 end ~ */


        /* ~ IStateDefiner<Block, BlockState> 接口实现 start ~ */
        public StateDefinition<Block, BlockState> StateDefinition
        {
            get => stateDefinition!;
            set => stateDefinition ??= value;
        }

        private StateDefinition<Block, BlockState>? stateDefinition;
        /* ~ IStateDefiner<Block, BlockState> 接口实现 start ~ */


        public override int GetHashCode()
        {
            return regData == null ? base.GetHashCode() : regData.GetHashCode();
        }

        public override string ToString()
        {
            return regData == null ? "[undefined]" : regData.id.ToString();
        }
    }
}
