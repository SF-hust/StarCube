using System;

namespace StarCube.Core.Component
{
    /// <summary>
    /// 组件的接口, 一般情况下应继承 Component<T> 类来创建新组件
    /// </summary>
    /// 也可以使用结构体继承此接口并实现其中属性和方法
    public interface IComponent
    {
        /// <summary>
        /// Component 所适用的 Owner 的类型
        /// </summary>
        public Type OwnerType { get; }

        /// <summary>
        /// Component 的类型
        /// </summary>
        public ComponentVariant Variant { get; }

        public ComponentType ComponentType => Variant.type;

        /// <summary>
        /// 是否允许一个 Owner 拥有多个 ComponentType 与此相同的组件
        /// </summary>
        /// 注意，所有 ComponentType 相同的组件，这个值也需要相同
        /// 目前这个属性恒为 false
        public bool AllowMultiple { get; }
    }

    public interface IComponent<T> : IComponent
        where T : class, IComponentHolder<T>
    {
        /// <summary>
        /// 该组件附着到的 owner
        /// </summary>
        public T Owner { get; }

        /// <summary>
        /// 该组件是否被附着到一个 owner 上
        /// </summary>
        public bool IsAttached { get; }

        /// <summary>
        /// 当 component 被添加到某容器中后调用此方法
        /// </summary>
        /// <param name="newOwner"></param>
        public void OnAddToOwner(T newOwner);

        /// <summary>
        /// 当 component 从某容器中删除后调用此方法
        /// </summary>
        public void OnRemoveFromOwner();

        /// <summary>
        /// 复制自身但不包含 Owner 信息
        /// </summary>
        /// <returns></returns>
        public IComponent<T> CloneWithoutOwner();
    }
}
