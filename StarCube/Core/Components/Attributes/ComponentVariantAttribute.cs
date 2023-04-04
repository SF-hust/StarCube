using System;

namespace StarCube.Core.Components.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class ComponentVariantAttribute : Attribute
    {
        public ComponentVariantAttribute(Type type)
        {
            this.type = type;
        }

        public Type type;
    }
}
