using System;

namespace StarCube.Core.Components.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class DisallowMultipleComponentAttribute : Attribute
    {
    }
}
