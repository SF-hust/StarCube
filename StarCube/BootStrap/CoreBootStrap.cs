using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using StarCube.Bootstrap.Attributes;

namespace StarCube.Bootstrap
{
    public class CoreBootstrap
    {
        public static void InitCore()
        {
            BootstrapAssembly(Assembly.GetAssembly(typeof(CoreBootstrap)));
        }

        public static void BootstrapAssembly(Assembly assembly)
        {
            foreach (BootstrapClassAttribute attr in assembly.GetCustomAttributes<BootstrapClassAttribute>())
            {
                RuntimeHelpers.RunClassConstructor(attr.type.TypeHandle);
            }

            foreach (BootstrapActionAttribute attr in assembly.GetCustomAttributes<BootstrapActionAttribute>())
            {
                attr.action.Invoke();
            }
        }
    }
}
