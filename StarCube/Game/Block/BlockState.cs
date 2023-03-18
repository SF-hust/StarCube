using System;

using StarCube.Utility;
using StarCube.Core.State;
using StarCube.Core.State.Property;
using StarCube.Core.Registry;

namespace StarCube.Game.Block
{
    public class BlockState : StateHolder<Block, BlockState>, IIntegerID
    {
        public static BlockState Create(Block block, StatePropertyList properties)
        {
            return new BlockState(block, properties);
        }

        public static void RunPostProcess()
        {
            int i = 0;
            foreach (Block block in Registries.BLOCK)
            {
                foreach (BlockState state in block.StateDefinition.states)
                {
                    state.SetIntegerID(i);
                    i++;
                }
            }
        }

        public Block Block { get => owner; }

        public int IntegerID => integerID;

        public void SetIntegerID(int integerID)
        {
            if(this.integerID != -1)
            {
                throw new InvalidOperationException($"integerID of BlockState of Block (\"{Block.ID}\") is already set");
            }
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
