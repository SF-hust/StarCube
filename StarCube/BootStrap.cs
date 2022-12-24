using System;
using StarCube.Core.Registry;

namespace StarCube
{
    public class BootStrap
    {
        public static void Boot()
        {
            InitRegistries();
        }

        private static void InitRegistries()
        {
            Registries.Init();
        }
    }
}
