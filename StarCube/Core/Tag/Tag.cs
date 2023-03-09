using System.Collections.Generic;
using System.Collections.Immutable;

using StarCube.Utility;

namespace StarCube.Core.Tag
{
    public abstract class Tag : IStringID
    {
        StringID IStringID.ID => id;

        public override string ToString()
        {
            return id.ToString();
        }

        public Tag(StringID id)
        {
            this.id = id;
        }

        /// <summary>
        /// Tag 的字符串id
        /// </summary>
        public readonly StringID id;
    }


    /// <summary>
    /// 标签可以附加到相应类型的对象上
    /// </summary>
    /// <typeparam name="T"> Tag 可以附加到的对象的类型 </typeparam>
    /// 一个 Tag 由其可以附加的类型与自己的 ResourceLocation 所唯一标识
    /// Tag 对象内存储着由其附加到的所有对象的引用
    /// 被 Tag 附着的对象并不保存 Tag 对象的引用，Tag 对象会保存在 TagManager 中
    /// Tag 对象不能自行构造，而必须通过 Builder 构造，以防意外的构造
    public class Tag<T> : Tag
        where T : class
    {
        /// <summary>
        /// 构造一个 Tag, 这个方法只能由内部调用
        /// </summary>
        /// <param name="id"></param>
        /// <param name="values"></param>
        internal Tag(StringID id, IEnumerable<T> values) : base(id)
        {
            elements = values.ToImmutableArray();
        }

        public override string ToString()
        {
            return typeof(T).Name + " " + id.ToString();
        }

        public override int GetHashCode()
        {
            return id.GetHashCode() + 31 * typeof(T).GetHashCode();
        }

        /// <summary>
        /// 此 Tag 应用于的对象
        /// </summary>
        public readonly ImmutableArray<T> elements;
    }
}
