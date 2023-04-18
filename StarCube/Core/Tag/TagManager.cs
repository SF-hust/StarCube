using System;
using System.Collections.Generic;
using System.Linq;

namespace StarCube.Core.Tag
{
    public class TagManager<T>
        where T : class, ITagHolder<T>
    {
        public static readonly IEnumerable<Tag<T>> EMPTY_TAGS = Enumerable.Empty<Tag<T>>();

        public static TagManager<T> Instance => instance ?? throw new NullReferenceException(nameof(Instance));

        private static TagManager<T>? instance;

        public IEnumerable<Tag<T>> Tags => tags;

        /// <summary>
        /// 获取一个 element 所有的 tag
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public IEnumerable<Tag<T>> GetTags(T element)
        {
            if(elementToTags.TryGetValue(element, out HashSet<Tag<T>> tag))
            {
                return tag;
            }
            return EMPTY_TAGS;
        }

        /// <summary>
        /// 某个 element 是否含有给定的 tag
        /// </summary>
        /// <param name="element"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool ElementHasTag(T element, Tag<T> tag)
        {
            return elementToTags.TryGetValue(element, out HashSet<Tag<T>> tagsOfElement) && tagsOfElement.Contains(tag);
        }


        internal TagManager(List<Tag<T>> tags)
        {
            this.tags = tags;
            elementToTags = new Dictionary<T, HashSet<Tag<T>>>();
            foreach (Tag<T> tag in tags)
            {
                foreach (T element in tag.elements)
                {
                    elementToTags[element].Add(tag);
                }
            }
            instance = this;
        }

        /// <summary>
        /// 此类别下所有 Tag，按 id 的命名空间优先顺序排序
        /// </summary>
        private readonly List<Tag<T>> tags;

        private readonly Dictionary<T, HashSet<Tag<T>>> elementToTags;
    }
}
