using System;

using StarCube.Utility;

namespace StarCube.Core.Components.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class ComponentTypeAttribute : Attribute
    {
        public ComponentTypeAttribute(StringID id, Type type)
        {
            this.id = id;
            this.type = type;
        }

        public readonly StringID id;

        public readonly Type type;
    }
}
