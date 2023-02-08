using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using StarCube.Resource;
using StarCube.Data.DependencyResolver;

namespace StarCube.Core.Tag
{
    /// <summary>
    /// 用来构造 Tag 对象
    /// </summary>
    public class TagBuilder<T> : IResolvedDataBuilder<StringID, TagData, Tag<T>>
        where T : class
    {
        /// <summary>
        /// 通过 StringID 获取对象的委托，此委托需要保证可多线程读
        /// </summary>
        private readonly Func<StringID, T?> elementGetter;

        public TagBuilder(Func<StringID, T?> elementGetter)
        {
            this.elementGetter = elementGetter;
        }

        public bool BuildResolvedData(TagData unresolvedData, Func<StringID, Tag<T>?> getResolvedData, [NotNullWhen(true)] out Tag<T>? resolvedData)
        {
            resolvedData = null;
            List<T> elements = new List<T>();

            foreach (TagData.Entry entry in unresolvedData.entries)
            {
                if (entry.entryType == TagData.Entry.EntryType.Element)
                {
                    T? element = elementGetter(entry.id);
                    if(element == null)
                    {
                        return false;
                    }
                    elements.Add(element);
                }
                else if (entry.entryType == TagData.Entry.EntryType.Tag)
                {
                    Tag<T>? tag = getResolvedData(entry.id);
                    if(tag == null)
                    {
                        return false;
                    }
                    elements.AddRange(tag.elements);
                }
                else
                {
                    return false;
                }
            }

            resolvedData = new Tag<T>(unresolvedData.id, elements);

            return true;
        }
    }
}
