using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using StarCube.Utility;
using StarCube.Data.DependencyResolver;

namespace StarCube.Core.Tag.Data
{
    /// <summary>
    /// 用来构造 Tag 对象
    /// </summary>
    public class TagBuilder<T> : IResolvedDataBuilder<TagData, Tag<T>>
        where T : class
    {
        public delegate bool TagHolderGetter(StringID id, [NotNullWhen(true)] out T? tagHolder);

        public bool BuildResolvedData(TagData unresolvedData, IResolvedDataBuilder<TagData, Tag<T>>.ResolvedDataGetter tryGetResolvedData, [NotNullWhen(true)] out Tag<T>? resolvedData)
        {
            resolvedData = null;
            List<T> elements = new List<T>();

            foreach (TagData.Entry entry in unresolvedData.entries)
            {
                if (entry.entryType == TagData.Entry.EntryType.Element)
                {
                    if(!tryGetTagHolder(entry.id, out T? element))
                    {
                        return false;
                    }

                    elements.Add(element);
                    continue;
                }

                if (entry.entryType == TagData.Entry.EntryType.Tag)
                {
                    if(!tryGetResolvedData(entry.id, out Tag<T>? tag))
                    {
                        return false;
                    }

                    elements.AddRange(tag.elements);
                    continue;
                }
                
                return false;
            }

            resolvedData = new Tag<T>(unresolvedData.id, elements);

            return true;
        }

        public TagBuilder(TagHolderGetter tagHolderGetter)
        {
            tryGetTagHolder = tagHolderGetter;
        }

        /// <summary>
        /// 通过 StringID 获取对象的委托，此委托需要保证可多线程读
        /// </summary>
        private readonly TagHolderGetter tryGetTagHolder;
    }
}
