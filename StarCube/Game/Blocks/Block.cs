using System;

using StarCube.Utility;
using StarCube.Core.Registry;
using StarCube.Core.State;
using StarCube.Core.Component;

namespace StarCube.Game.Blocks
{
    public class Block :
        IRegistryEntry<Block>,
        IStateDefiner<Block, BlockState>,
        IComponentHolder<Block>
    {
        /* ~ IRegistryEntry<Block> 接口实现 start ~ */
        public RegistryEntryData<Block> RegistryEntryData
        {
            get => IRegistryEntry<Block>.RegistryEntryGetHelper(regData);
            set => IRegistryEntry<Block>.RegistryEntrySetHelper(ref regData, value);
        }
        private RegistryEntryData<Block>? regData = null;

        public virtual Type AsEntryType => typeof(Block);
        public Registry<Block> Registry => regData!.registry;
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
        /* ~ IStateDefiner<Block, BlockState> 接口实现 end ~ */


        /* ~ IComponentHolder<Block> 接口实现 start ~ */
        public ComponentContainer<Block> Components => throw new NotImplementedException();
        /* ~ IComponentHolder<Block> 接口实现 end ~ */

        public override int GetHashCode()
        {
            return regData == null ? base.GetHashCode() : regData.GetHashCode();
        }

        public override string ToString()
        {
            return regData == null ? "[undefined]" : regData.id.ToString();
        }

        public Block()
        {
        }
    }
}
