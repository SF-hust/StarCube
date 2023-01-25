using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace StarCube.BootStrap.ForceContructService
{
    public static class ForceConstructService
    {
        public static void ForceConstructClass(Type type)
        {
            RuntimeHelpers.RunClassConstructor(type.TypeHandle);
        }

        public static void ForceConstructClassesInAssembly(Assembly assembly)
        {
            foreach(Type type in assembly.GetTypes())
            {
                if(type.GetCustomAttribute(typeof(ConstructInBootStrapAttribute)) == null)
                {
                    continue;
                }
                ForceConstructClass(type);
            }
        }
    }
}
