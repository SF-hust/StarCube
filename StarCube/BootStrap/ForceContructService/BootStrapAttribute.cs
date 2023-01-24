using System;
using System.Collections.Generic;
using System.Text;

namespace StarCube.BootStrap.ForceContructService
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class BootStrapAttribute : Attribute
    {
    }
}
