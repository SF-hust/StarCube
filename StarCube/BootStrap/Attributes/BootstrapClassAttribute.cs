using System;

namespace StarCube.Bootstrap.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public sealed class RegisterBootstrapClassAttribute : Attribute
    {
        public RegisterBootstrapClassAttribute(Type type)
        {
            this.type = type;
        }

        public readonly Type type;
    }
}
