using System;

using StarCube.Utility;
using StarCube.Core.Registries;
using StarCube.Core.State;
using StarCube.Core.Component;
using StarCube.Core.State.Property;
using System.Collections.Generic;

namespace StarCube.Game.Blocks
{
    public class Block :
        RegistryEntry<Block>,
        IStateDefiner<Block, BlockState>,
        IComponentHolder<Block>
    {
        /* ~ Block 属性 start ~ */
        public bool IsAir => properties.air;
        public bool IsSolid => properties.solid;
        public float Hardness => properties.hardness;
        public float Strength => properties.strength;
        /* ~ Block 属性 end ~ */


        /* ~ IStateDefiner<Block, BlockState> 接口实现 start ~ */
        public StateDefinition<Block, BlockState> StateDefinition
        {
            get => stateDefinition!;
            set => stateDefinition ??= value;
        }
        private StateDefinition<Block, BlockState>? stateDefinition;
        /* ~ IStateDefiner<Block, BlockState> 接口实现 end ~ */


        /* ~ IComponentHolder<Block> 接口实现 start ~ */
        public ComponentContainer<Block> Components => components;
        /* ~ IComponentHolder<Block> 接口实现 end ~ */

        public Block(StringID id, in BlockProperties properties)
            : base(BuiltinRegistries.BLOCK, id)
        {
            this.properties = properties;
            components = new ComponentContainer<Block>(this);
            stateDefinition = StateDefinition<Block, BlockState>.BuildSingle(this, BlockState.Create);
        }

        public Block(StringID id, in BlockProperties properties, List<StatePropertyEntry> entries)
            : base(BuiltinRegistries.BLOCK, id)
        {
            this.properties = properties;
            components = new ComponentContainer<Block>(this);
            stateDefinition = StateDefinition<Block, BlockState>.BuildFromPropertyEntryList(this, BlockState.Create, entries);
        }

        private readonly BlockProperties properties;

        private readonly ComponentContainer<Block> components;
    }
}
