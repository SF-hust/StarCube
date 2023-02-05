using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace StarCube.Core.Component
{
    public class ComponentHolder<T>
        where T : class, IComponentHolder<T>
    {
        private readonly Dictionary<ComponentType, IComponent<T>> componentsByType = new Dictionary<ComponentType, IComponent<T>>();

        private readonly T owner;

        public ComponentHolder(T owner)
        {
            this.owner = owner;
        }

        /// <summary>
        /// 容器中所有组件的集合
        /// </summary>
        public IEnumerable<IComponent> Values => componentsByType.Values;

        public IComponent<T> this[ComponentType type]
        {
            get => TryGet(type, out IComponent<T>? component) ? component : throw new KeyNotFoundException();
        }

        /// <summary>
        /// 尝试查找并返回指定类型的 component
        /// </summary>
        /// <param name="componentType"></param>
        /// <param name="component"></param>
        /// <returns></returns>
        public bool TryGet(ComponentType type, [NotNullWhen(true)] out IComponent<T>? component)
        {
            return componentsByType.TryGetValue(type, out component);
        }

        /// <summary>
        /// 尝试向容器中添加某个 component
        /// </summary>
        /// <param name="component"></param>
        /// <returns>若容器中已存在相同类型的组件，则返回 false</returns>
        public bool TryAdd(IComponent<T> component)
        {
            bool result = componentsByType.TryAdd(component.ComponentType, component);
            if(result)
            {
                component.OnAddToOwner(owner);
            }
            return result;
        }

        /// <summary>
        /// 尝试替换容器中的某个 component
        /// </summary>
        /// <param name="component"></param>
        /// <returns>若容器中不存在相同类型的组件，或替换前后组件是同一对象，则返回 false</returns>
        public bool TryUpdate(IComponent<T> component)
        {
            if (!componentsByType.TryGetValue(component.ComponentType, out IComponent<T> oldComp) ||
                object.ReferenceEquals(oldComp, component))
            {
                return false;
            }
            componentsByType[component.ComponentType] = component;
            oldComp.OnRemoveFromOwner();
            component.OnAddToOwner(owner);
            return true;
        }

        /// <summary>
        /// 直接设置某类型的 component, 不管同类组件是否已经在容器中存在
        /// </summary>
        /// <param name="component"></param>
        public void Set(IComponent<T> component)
        {
            if (componentsByType.TryGetValue(component.ComponentType, out IComponent<T> oldComp))
            {
                if (object.ReferenceEquals(oldComp, component))
                {
                    return;
                }
                componentsByType[component.ComponentType] = component;
                oldComp.OnRemoveFromOwner();
                component.OnAddToOwner(owner);
            }
            else
            {
                componentsByType[component.ComponentType] = component;
                component.OnAddToOwner(owner);
            }
        }

        /// <summary>
        /// 检测容器中是否存在某类型组件
        /// </summary>
        /// <param name="componentType"></param>
        /// <returns></returns>
        public bool ContainsType(ComponentType type)
        {
            return componentsByType.ContainsKey(type);
        }

        /// <summary>
        /// 检测容器中是否有指定组件同 ComponentType 的组件
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public bool ContainsSameType(IComponent<T> component)
        {
            return ContainsType(component.ComponentType);
        }

        /// <summary>
        /// 尝试删除指定类型的 component
        /// </summary>
        /// <param name="type"></param>
        /// <returns>如果不存在此类型的 component 则返回 false</returns>
        public bool Remove(ComponentType type)
        {
            if (componentsByType.Remove(type, out IComponent<T>? com))
            {
                com.OnRemoveFromOwner();
                return true;
            }
            return false;
        }
    }
}
