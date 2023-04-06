using System;

using StarCube.Utility;
using StarCube.Utility.Container;
using StarCube.Core.States;
using StarCube.Core.States.Property;
using StarCube.Core.Registries;
using System.Text;

namespace StarCube.Game.Blocks
{
    public class BlockState : State<Block, BlockState>, IIntegerID
    {
        public static IIDMap<BlockState> GlobalBlockStateIDMap => blockStates ?? throw new NullReferenceException();

        private static IIDMap<BlockState>? blockStates = null;

        public static BlockState Create(Block block, StatePropertyList properties)
        {
            return new BlockState(block, properties);
        }

        public static void BuildGlobalBlockStateIDMap()
        {
            int i = 0;
            IntegerIDMap<BlockState> blockStates = new IntegerIDMap<BlockState>();
            foreach (Block block in BuiltinRegistries.BLOCK)
            {
                foreach (BlockState state in block.StateDefinition.states)
                {
                    state.SetIntegerID(i);
                    blockStates.Add(state);
                    i++;
                }
            }
            BlockState.blockStates = blockStates;
        }

        public Block Block { get => owner; }

        public int IntegerID => integerID;

        private void SetIntegerID(int integerID)
        {
            this.integerID = integerID;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Block, propertyList);
        }

        public override string ToString()
        {
            StringBuilder builder = StringUtil.StringBuilder;
            builder.Append(Block.ID).Append('[').Append(integerID).Append(']');
            return builder.ToString();
        }

        private BlockState(Block block, StatePropertyList properties)
            : base(block, properties)
        {
            integerID = -1;
        }

        private int integerID;
    }
}
