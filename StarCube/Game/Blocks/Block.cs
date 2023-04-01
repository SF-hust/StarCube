using System;

using StarCube.Utility;
using StarCube.Core.Registries;
using StarCube.Core.State;
using StarCube.Core.Component;

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
        public double Hardness => properties.hardness;
        public double Strength => properties.strength;
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
        public ComponentContainer<Block> Components => throw new NotImplementedException();
        /* ~ IComponentHolder<Block> 接口实现 end ~ */


        public Block(StringID id, BlockProperties properties)
            : base(BuiltinRegistries.BLOCK, id)
        {
            this.properties = properties;
        }

        private readonly BlockProperties properties;
    }
}
