using System;
using System.Collections.Generic;
using System.Text;

namespace StarCube.Core.Tag
{
    public interface ITagHolder
    {
    }

    public interface ITagHolder<T> : ITagHolder
        where T : class, ITagHolder<T>
    {
        public bool HasTag(Tag<T> tag);

        public TagHolder<T> TagHolder { get; set; }
    }

}
