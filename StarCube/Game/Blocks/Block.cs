﻿using System;

using StarCube.Utility;
using StarCube.Core.Registries;
using StarCube.Core.States;
using StarCube.Core.Components;
using StarCube.Core.States.Property;
using System.Collections.Generic;

namespace StarCube.Game.Blocks
{
    public class Block :
        RegistryEntry<Block>,
        IStateOwner<Block, BlockState>,
        IComponentHolder<Block>
    {
        /* ~ Block 属性 start ~ */
        public bool IsAir => properties.air;
        public bool IsSolid => properties.solid;
        public float Hardness => properties.hardness;
        public float Strength => properties.strength;
        /* ~ Block 属性 end ~ */


        /* ~ IStateOwner<Block, BlockState> 接口实现 start ~ */
        public StateDefinition<Block, BlockState> StateDefinition => stateDefinition;
        /* ~ IStateOwner<Block, BlockState> 接口实现 end ~ */


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
            stateDefinition = StateDefinition<Block, BlockState>.BuildFromPropertyEntryList(this, BlockState.Create, entries);
            components = new ComponentContainer<Block>(this);
        }

        private readonly BlockProperties properties;

        private readonly StateDefinition<Block, BlockState> stateDefinition;

        private readonly ComponentContainer<Block> components;
    }
}
