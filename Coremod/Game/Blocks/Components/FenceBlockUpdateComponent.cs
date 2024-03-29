﻿using System;
using System.Diagnostics.CodeAnalysis;

using Newtonsoft.Json.Linq;

using LiteDB;

using StarCube.Utility;
using StarCube.Utility.Math;
using StarCube.Game.Blocks;
using StarCube.Game.Blocks.Components;
using StarCube.Game.Blocks.StateProperties;
using StarCube.Game.Levels;

namespace StarCube.Coremod.Game.Blocks.Components
{
    public sealed class FenceBlockUpdateComponent : BlockUpdateComponent
    {
        public sealed class Type : BlockComponentType<FenceBlockUpdateComponent>
        {
            protected override void StoreTo(FenceBlockUpdateComponent component, BsonDocument bson)
            {
            }

            protected override bool TryCreateFactoryFrom(JObject json, [NotNullWhen(true)] out Func<FenceBlockUpdateComponent>? factory)
            {
                factory = () => new FenceBlockUpdateComponent();
                return true;
            }

            protected override bool TryRestoreFrom(BsonDocument bson, [NotNullWhen(true)] out FenceBlockUpdateComponent? component)
            {
                component = new FenceBlockUpdateComponent();
                return true;
            }

            public Type() : base(StringID.Create(Constants.DEFAULT_NAMESPACE, "fence_update"))
            {
            }
        }

        public override bool OnUpdate(Level level, BlockPos blockPos, BlockState blockState)
        {
            BlockState newState = blockState;

            bool shouldConnectNorth = level.TryGetBlockState(blockPos.North, out BlockState? north) &&
                (north.Block.IsSolid || north.propertyList.Contains(BlockStateProperties.FENCE_SOUTH));
            int northPart = blockState.propertyList.GetValueIndex(BlockStateProperties.FENCE_NORTH);
            if (northPart == 0 && shouldConnectNorth || northPart == 1 && !shouldConnectNorth)
            {
                newState = newState.CycleProperty(BlockStateProperties.FENCE_NORTH);
            }

            bool shouldConnectSouth = level.TryGetBlockState(blockPos.South, out BlockState? south) &&
                (south.Block.IsSolid || south.propertyList.Contains(BlockStateProperties.FENCE_NORTH));
            int southPart = blockState.propertyList.GetValueIndex(BlockStateProperties.FENCE_SOUTH);
            if (southPart == 0 && shouldConnectSouth || southPart == 1 && !shouldConnectSouth)
            {
                newState = newState.CycleProperty(BlockStateProperties.FENCE_SOUTH);
            }

            bool shouldConnectEast = level.TryGetBlockState(blockPos.East, out BlockState? east) &&
                (east.Block.IsSolid || east.propertyList.Contains(BlockStateProperties.FENCE_WEST));
            int eastPart = blockState.propertyList.GetValueIndex(BlockStateProperties.FENCE_EAST);
            if (eastPart == 0 && shouldConnectEast || eastPart == 1 && !shouldConnectEast)
            {
                newState = newState.CycleProperty(BlockStateProperties.FENCE_EAST);
            }

            bool shouldConnectWest = level.TryGetBlockState(blockPos.West, out BlockState? west) &&
                (west.Block.IsSolid || west.propertyList.Contains(BlockStateProperties.FENCE_EAST));
            int westPart = blockState.propertyList.GetValueIndex(BlockStateProperties.FENCE_WEST);
            if (westPart == 0 && shouldConnectWest || westPart == 1 && !shouldConnectWest)
            {
                newState = newState.CycleProperty(BlockStateProperties.FENCE_WEST);
            }

            if(newState == blockState)
            {
                return false;
            }

            if (newState.Block.Components.Contains<FenceBlockUpdateComponent>())
            {

            }

            level.TrySetBlockState(blockPos, newState);
            return true;
        }

        public FenceBlockUpdateComponent() : base(BlockComponentTypes.FenceUpdate)
        {
        }
    }
}
