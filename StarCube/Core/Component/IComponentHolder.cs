using System;
using System.Collections.Generic;
using System.Text;

namespace StarCube.Core.Component
{
    public interface IComponentHolder
    {
    }

    public interface IComponentHolder<T> : IComponentHolder
        where T : class, IComponentHolder<T>
    {
        public ComponentHolder<T> Components { get; }
    }
}
