using System.Reflection;
using StarCube.BootStrap.ForceContructService;

namespace StarCube.BootStrap
{
    public class BootStrap
    {
        public static void Boot()
        {
            ForceConstructService.ForceConstructClassesInAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
