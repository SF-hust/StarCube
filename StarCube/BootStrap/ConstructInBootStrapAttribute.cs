using System;

namespace StarCube.BootStrap
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ConstructInBootStrapAttribute : Attribute
    {
    }
}
