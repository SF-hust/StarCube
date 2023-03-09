using System;

namespace StarCube.Core.Mod
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
