using System;
using System.Reflection;
using System.Runtime.CompilerServices;

using StarCube.Bootstrap.Attributes;

namespace StarCube.Bootstrap
{
    public class CoreBootstrap
    {
        public static void InitCore()
        {
            EnsureConstructClassesInAssembly(Assembly.GetExecutingAssembly());
        }

        public static void EnsureConstructClass(Type type)
        {
            RuntimeHelpers.RunClassConstructor(type.TypeHandle);
        }

        public static void EnsureConstructClassesInAssembly(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.GetCustomAttribute(typeof(BootstrapClassAttribute)) == null)
                {
                    continue;
                }
                EnsureConstructClass(type);
            }
        }
    }
}
