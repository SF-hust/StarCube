using System;

namespace StarCube.Mod.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ModAttribute : Attribute
    {
        public string modid;

        public ModAttribute(string modid)
        {
            this.modid = modid;
        }
    }
}
