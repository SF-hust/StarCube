using System;

namespace StarCube.Core.Tag.Attributes
{
    /// <summary>
    /// 用于拥有特性 TagDeclareAttribute 的类中的 Tag 声明
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class TagAttribute : Attribute
    {
        public readonly string name;

        public TagAttribute(string name)
        {
            this.name = name;
        }
    }
}
