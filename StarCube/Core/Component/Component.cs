using System;
using System.Diagnostics;

namespace StarCube.Core.Component
{
    public abstract class Component<T> : IComponent<T>
        where T : class, IComponentHolder<T>
    {
        public Type OwnerType => typeof(T);

        public abstract ComponentVariant Variant { get; }

        public bool AllowMultiple => false;

        public T Owner => owner ?? throw new NullReferenceException();

        private T? owner = null;

        public bool IsAttached => owner != null;

        public Component()
        {
        }

        public virtual void OnAddToOwner(T newOwner)
        {
            if (newOwner == owner)
            {
                return;
            }
            Debug.Assert(owner == null);
            owner = newOwner;
        }

        public virtual void OnRemoveFromOwner()
        {
            if (owner == null)
            {
                return;
            }
            owner = null;
        }

        public abstract IComponent<T> CloneWithoutOwner();
    }
}
