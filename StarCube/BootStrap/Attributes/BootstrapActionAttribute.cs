using System;

namespace StarCube.Bootstrap.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class BootstrapActionAttribute : Attribute
    {
        public BootstrapActionAttribute(Action action)
        {
            this.action = action;
        }

        public readonly Action action;
    }
}
