using System;

namespace StarCube.Mods.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class RegisterModAttribute : Attribute
    {
        public RegisterModAttribute(string modid, Type type)
        {
            this.modid = modid;
            this.type = type;
        }

        public readonly string modid;

        public readonly Type type;
    }
}
