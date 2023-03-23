using System;
using System.Collections.Generic;
using System.Text;

namespace StarCube.BootStrap.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class BootstrapMethodAttribute : Attribute
    {
    }
}
