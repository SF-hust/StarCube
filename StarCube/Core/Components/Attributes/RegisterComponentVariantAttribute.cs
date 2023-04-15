using System;

namespace StarCube.Core.Components.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class RegisterComponentVariantAttribute : Attribute
    {
        public RegisterComponentVariantAttribute(Type type)
        {
            this.type = type;
        }

        public Type type;
    }
}
