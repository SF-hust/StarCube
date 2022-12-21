using System;

namespace StarCube.Core.Tag.Attributes
{
    /// <summary>
    /// 用于标识一个类被用于声明 Tag
    /// </summary>
    /// 由于 C# 8 不支持泛型 Attribute，需要将 targetClass 作为参数传递
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class TagDeclareAttribute : Attribute
    {
        /// <summary>
        /// Tag<T> 中的 T 类型
        /// </summary>
        public readonly Type targetClass;

        /// <summary>
        /// 其中 Tag 的 id 的 namespace，在大多数情况下会是本 mod 的 modid， 当然也可以不是
        /// </summary>
        public readonly string modid;

        public TagDeclareAttribute(Type targetClass, string modid)
        {
            this.modid = modid;
            this.targetClass = targetClass;
        }
    }
}
