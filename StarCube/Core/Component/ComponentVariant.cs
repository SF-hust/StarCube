using System;

using StarCube.Resource;

namespace StarCube.Core.Component
{
    public abstract class ComponentVariant
    {
        public readonly ComponentType type;

        public readonly StringID id;

        /// <summary>
        /// Component 的实际 C# 类型
        /// </summary>
        public readonly Type underlyingType;

        public ComponentVariant(ComponentType type, StringID id, Type underlyingType)
        {
            this.type = type;
            this.id = id;
            this.underlyingType = underlyingType;
        }
    }

    public class ComponentVariant<O, C> : ComponentVariant
        where O : class, IComponentHolder<O>
        where C : IComponent<O>
    {
        private readonly Func<C> factory;

        public ComponentVariant(ComponentType<O, C> type, StringID id, Type underlyingType, Func<C> factory) : base(type, id, underlyingType)
        {
            this.factory = factory;
        }

        public C Create()
        {
            return factory();
        }
    }
}
