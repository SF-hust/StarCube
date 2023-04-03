using System;

using StarCube.Utility;
using StarCube.Utility.Container;
using StarCube.Core.State;
using StarCube.Core.State.Property;
using StarCube.Core.Registries;

namespace StarCube.Game.Blocks
{
    public class BlockState : StateHolder<Block, BlockState>, IIntegerID
    {
        public static IIDMap<BlockState> GlobalBlockStateIDMap => blockStates ?? throw new NullReferenceException();

        private static IIDMap<BlockState>? blockStates = null;

        public static BlockState Create(Block block, StatePropertyList properties)
        {
            return new BlockState(block, properties);
        }

        public static void GatherBlockStates()
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

        public override string ToString()
        {
            return Block.ToString() + " " + integerID.ToString();
        }

        private BlockState(Block block, StatePropertyList properties)
            : base(block, properties)
        {
            integerID = -1;
        }

        private int integerID;
    }
}
