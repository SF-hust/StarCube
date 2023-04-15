using System;

namespace StarCube.Core.Components.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class RegisterComponentTypeAttribute : Attribute
    {
        public RegisterComponentTypeAttribute(Type type)
        {
            this.type = type;
        }

        public readonly Type type;
    }
}
