using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using LiteDB;

namespace StarCube.Core.Components
{
    public sealed class ComponentContainer<O>
        where O : class, IComponentOwner<O>
    {
        public int Count => components.Count;

        public IEnumerable<Component<O>> Components => components;

        /// <summary>
        /// 是否存在指定类型的 Component
        /// </summary>
        /// <typeparam name="C"></typeparam>
        /// <returns></returns>
        public bool Contains<C>()
            where C : Component<O>
        {
            foreach (Component<O> comp in components)
            {
                if (comp is C)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 尝试获取第一个指定类型的 Component
        /// </summary>
        /// <typeparam name="C"></typeparam>
        /// <param name="component"></param>
        /// <returns></returns>
        public bool TryGet<C>([NotNullWhen(true)] out C? component)
            where C : Component<O>
        {
            foreach (Component<O> comp in components)
            {
                if (comp is C c)
                {
                    component = c;
                    return true;
                }
            }

            component = null;
            return false;
        }

        /// <summary>
        /// 尝试获取所有指定类型的 Component
        /// </summary>
        /// <typeparam name="C"></typeparam>
        /// <param name="components"></param>
        /// <returns></returns>
        public int Gets<C>(List<C> components)
            where C : Component<O>
        {
            int count = 0;
            foreach (Component<O> comp in this.components)
            {
                if(comp is C component)
                {
                    components.Add(component);
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// 尝试添加一个 Component
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public bool TryAdd(Component<O> component)
        {
            components.Add(component);
            component.SetOwner(owner);
            return true;
        }

        /// <summary>
        /// 尝试移除第一个指定类型的 Component
        /// </summary>
        /// <typeparam name="C"></typeparam>
        /// <returns></returns>
        public bool Remove<C>()
            where C : Component<O>
        {
            for (int i = 0; i < components.Count; ++i)
            {
                if (components[i] is C)
                {
                    components[i].ResetOwner();
                    components.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 从后往前移除所有组件
        /// </summary>
        /// <returns></returns>
        public int RemoveAll()
        {
            int count = components.Count;

            for (int i = count - 1; i >= 0; --i)
            {
                components[i].ResetOwner();
                components.RemoveAt(i);
            }

            return count;
        }

        /// <summary>
        /// 尝试从后往前移除所有指定类型的 Component
        /// </summary>
        /// <typeparam name="C"></typeparam>
        /// <returns></returns>
        public int RemoveAll<C>()
            where C : Component<O>
        {
            int count = 0;

            for (int i = components.Count - 1; i >= 0; --i)
            {
                if (components[i] is C)
                {
                    components[i].ResetOwner();
                    components.RemoveAt(i);
                    count++;
                }
            }

            return count;
        }

        public ComponentContainer(O owner)
        {
            this.owner = owner;
            components = new List<Component<O>>();
        }

        private readonly O owner;

        private readonly List<Component<O>> components;
    }

    public static class ComponentContainerExtension
    {
        public static void StoreTo<O>(this ComponentContainer<O> container, string key, BsonDocument bson)
            where O : class, IComponentOwner<O>
        {
            BsonArray array = new BsonArray();
            foreach (Component<O> component in container.Components)
            {
                BsonDocument doc = new BsonDocument();
                component.type.StoreComponentTo(component, bson);
                array.Add(doc);
            }
            bson.Add(key, array);
        }
    }
}
