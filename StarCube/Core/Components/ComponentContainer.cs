using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace StarCube.Core.Components
{
    public class ComponentContainer<O>
        where O : class, IComponentOwner<O>
    {
        public IEnumerable<Component<O>> Components => components;

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

        public bool Contains(ComponentType<O> type)
        {
            foreach (Component<O> comp in components)
            {
                if (ReferenceEquals(comp.Type, type))
                {
                    return true;
                }
            }

            return false;
        }

        public bool Contains(ComponentVariant<O> variant)
        {
            foreach (Component<O> comp in components)
            {
                if (ReferenceEquals(comp.Variant, variant))
                {
                    return true;
                }
            }

            return false;
        }

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

        public bool TryGet<C>(ComponentType<O, C> type, [NotNullWhen(true)] out C? component)
            where C : Component<O>
        {
            foreach (Component<O> comp in components)
            {
                if (ReferenceEquals(comp.Type, type))
                {
                    component = (C)comp;
                    return true;
                }
            }

            component = null;
            return false;
        }

        public bool TryGet(ComponentType<O> type, [NotNullWhen(true)] out Component<O>? component)
        {
            foreach (Component<O> comp in components)
            {
                if (ReferenceEquals(comp.Type, type))
                {
                    component = comp;
                    return true;
                }
            }

            component = null;
            return false;
        }

        public bool TryGet<C>(ComponentVariant<O, C> variant, [NotNullWhen(true)] out C? component)
            where C : Component<O>
        {
            foreach (Component<O> comp in components)
            {
                if (ReferenceEquals(comp.Variant, variant))
                {
                    component = (C)comp;
                    return true;
                }
            }

            component = null;
            return false;
        }

        public bool TryGet(ComponentVariant<O> variant, [NotNullWhen(true)] out Component<O>? component)
        {
            foreach (Component<O> comp in components)
            {
                if (ReferenceEquals(comp.Variant, variant))
                {
                    component = comp;
                    return true;
                }
            }

            component = null;
            return false;
        }

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

        public int Gets<C>(ComponentType<O, C> type, List<C> components)
            where C : Component<O>
        {
            int count = 0;
            foreach (Component<O> comp in components)
            {
                if (ReferenceEquals(comp.Type, type))
                {
                    components.Add((C)comp);
                    count++;
                }
            }

            return count;
        }

        public int Gets(ComponentType<O> type, List<Component<O>> components)
        {
            int count = 0;
            foreach (Component<O> comp in components)
            {
                if (ReferenceEquals(comp.Type, type))
                {
                    components.Add(comp);
                    count++;
                }
            }

            return count;
        }

        public int Gets<C>(ComponentVariant<O, C> variant, List<C> components)
            where C : Component<O>
        {
            int count = 0;
            foreach (Component<O> comp in components)
            {
                if (ReferenceEquals(comp.Variant, variant))
                {
                    components.Add((C)comp);
                    count++;
                }
            }

            return count;
        }

        public int Gets(ComponentVariant<O> variant, List<Component<O>> components)
        {
            int count = 0;
            foreach (Component<O> comp in components)
            {
                if (ReferenceEquals(comp.Variant, variant))
                {
                    components.Add(comp);
                    count++;
                }
            }

            return count;
        }

        public bool TryAdd(Component<O> component)
        {
            if (!component.Type.allowMultiple && Contains(component.Type))
            {
                return false;
            }

            components.Add(component);
            component.SetOwner(owner);
            component.OnAddToOwner();
            return true;
        }

        public bool Remove(Component<O> component)
        {
            int i = components.IndexOf(component);
            if (i == -1)
            {
                return false;
            }

            component.OnRemoveFromOwner();
            component.ResetOwner();
            components.RemoveAt(i);

            return true;
        }

        public bool Remove<C>()
            where C : Component<O>
        {
            for (int i = 0; i < components.Count; ++i)
            {
                if (components[i] is C)
                {
                    components[i].OnRemoveFromOwner();
                    components[i].ResetOwner();
                    components.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public bool Remove(ComponentType<O> type)
        {
            for (int i = 0; i < components.Count; ++i)
            {
                if (ReferenceEquals(components[i].Type, type))
                {
                    components[i].OnRemoveFromOwner();
                    components[i].ResetOwner();
                    components.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public bool Remove(ComponentVariant<O> variant)
        {
            for (int i = 0; i < components.Count; ++i)
            {
                if (ReferenceEquals(components[i].Variant, variant))
                {
                    components[i].OnRemoveFromOwner();
                    components[i].ResetOwner();
                    components.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public int RemoveAll()
        {
            int count = components.Count;

            for (int i = count - 1; i >= 0; --i)
            {
                components[i].OnRemoveFromOwner();
                components[i].ResetOwner();
                components.RemoveAt(i);
            }

            return count;
        }

        public int RemoveAll<C>()
            where C : Component<O>
        {
            int count = 0;

            for (int i = components.Count - 1; i >= 0; --i)
            {
                if (components[i] is C)
                {
                    components[i].OnRemoveFromOwner();
                    components[i].ResetOwner();
                    components.RemoveAt(i);
                    count++;
                }
            }

            return count;
        }

        public int RemoveAll(ComponentType<O> type)
        {
            int count = 0;

            for (int i = components.Count - 1; i >= 0; --i)
            {
                if (ReferenceEquals(components[i].Type, type))
                {
                    components[i].OnRemoveFromOwner();
                    components[i].ResetOwner();
                    components.RemoveAt(i);
                    count++;
                }
            }

            return count;
        }

        public int RemoveAll(ComponentVariant<O> variant)
        {
            int count = 0;

            for (int i = components.Count - 1; i >= 0; --i)
            {
                if (ReferenceEquals(components[i].Variant, variant))
                {
                    components[i].OnRemoveFromOwner();
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
}
