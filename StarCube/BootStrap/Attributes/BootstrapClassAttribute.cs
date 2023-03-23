using System;

namespace StarCube.BootStrap.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class BootstrapClassAttribute : Attribute
    {
    }
}
