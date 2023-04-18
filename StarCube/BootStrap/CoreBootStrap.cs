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
            foreach (RegisterBootstrapClassAttribute attr in assembly.GetCustomAttributes<RegisterBootstrapClassAttribute>())
            {
                RuntimeHelpers.RunClassConstructor(attr.type.TypeHandle);
            }
        }
    }
}
