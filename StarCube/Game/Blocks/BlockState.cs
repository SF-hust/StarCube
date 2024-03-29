﻿using System;
using System.Text;

using LiteDB;

using StarCube.Utility;
using StarCube.Utility.Container;
using StarCube.Core.States;
using StarCube.Core.States.Property;
using StarCube.Core.Registries;

namespace StarCube.Game.Blocks
{
    public class BlockState : State<Block, BlockState>, IIntegerID
    {
        private static readonly Lazy<IIDMap<BlockState>> blockStates = new Lazy<IIDMap<BlockState>>(BuildGlobalBlockStateIDMap, true);

        public static IIDMap<BlockState> GlobalBlockStateIDMap => blockStates.Value;


        public static BlockState Create(Block block, StatePropertyList properties, int localID)
        {
            return new BlockState(block, properties, localID);
        }

        private static IIDMap<BlockState> BuildGlobalBlockStateIDMap()
        {
            int i = 0;
            IntegerIDMap<BlockState> blockStates = new IntegerIDMap<BlockState>();
            if (BuiltinRegistries.Block.Count == 0)
            {
                throw new InvalidOperationException("Block has not been registered");
            }
            foreach (Block block in BuiltinRegistries.Block)
            {
                foreach (BlockState state in block.StateDefinition.states)
                {
                    state.SetIntegerID(i);
                    blockStates.Add(state);
                    i++;
                }
            }
            return blockStates;
        }

        public static BsonValue ToBson(BlockState blockState)
        {
            return new BsonDocument
            {
                { "id", blockState.owner.ID.idString },
                { "local", blockState.localID }
            };
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

        private BlockState(Block block, StatePropertyList properties, int localID)
            : base(block, properties, localID)
        {
            integerID = -1;
        }

        private int integerID;
    }
}
