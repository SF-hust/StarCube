﻿using StarCube.Core.Registries;
using StarCube.Core.Components;

namespace StarCube.Game.Entities.Components
{
    public static class BuiltinEntityComponentTypes
    {
        public static ComponentType<Entity> Level = new LevelComponent.Type();
        public static ComponentType<Entity> Player = new PlayerComponent.Type();

        static BuiltinEntityComponentTypes()
        {
            DeferredRegister<ComponentType<Entity>> deferredRegister = new DeferredRegister<ComponentType<Entity>>(BuiltinRegistries.EntityComponentType);
            deferredRegister.Register(Level);
            deferredRegister.Register(Player);
        }
    }
}
