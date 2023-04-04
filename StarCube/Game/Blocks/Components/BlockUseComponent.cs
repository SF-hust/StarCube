﻿using StarCube.Utility;
using StarCube.Utility.Math;
using StarCube.Core.Registries;
using StarCube.Core.Components;
using StarCube.Core.Components.Attributes;
using StarCube.Game.Levels;

namespace StarCube.Game.Blocks.Components
{
    public abstract class BlockUseComponent : Component<Block>
    {
        public static readonly StringID ComponentTypeID = StringID.Create(Constants.DEFAULT_NAMESPACE, "random_tick");

        public static readonly ComponentType<Block, RandomTickComponent> COMPONENT_TYPE = 
            new ComponentType<Block, RandomTickComponent>(BuiltinRegistries.BLOCK_COMPONENT_TYPE, ComponentTypeID);

        public abstract override ComponentVariant<Block> Variant { get; }

        public abstract void OnUse(Level level, BlockPos pos, Entities.Entity user);
    }
}
