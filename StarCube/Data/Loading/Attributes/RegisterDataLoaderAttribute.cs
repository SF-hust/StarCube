using System;

namespace StarCube.Data.Loading.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class RegisterDataLoaderAttribute : Attribute
    {
        public RegisterDataLoaderAttribute(Type type)
        {
            this.type = type;
        }

        public readonly Type type;
    }
}
