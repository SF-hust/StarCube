using System;
using System.Collections.Generic;
using System.Text;

namespace StarCube.Game.Worlds
{
    public static class ServerWorldActions
    {
        public static Action<ServerWorld> Init = (world) => world.Init();

        public static Action<ServerWorld> Tick = (world) => world.Tick();

        public static Action<ServerWorld> Reset = (world) => world.Reset();

        public static Action<ServerWorld> Save = (world) => world.Save();
    }
}
