using System;

namespace StarCube.Core.Components
{
    public abstract class Component<O>
        where O : class, IComponentOwner<O>
    {
        /* ~ Owner ~ */
        public bool Attached => owner != null;

        public O Owner => owner ?? throw new NullReferenceException();

        public void SetOwner(O newOwner)
        {
            if (newOwner == owner)
            {
                return;
            }
            owner = newOwner;
            OnAddToOwner();
        }

        public void ResetOwner()
        {
            if (owner == null)
            {
                return;
            }
            OnRemoveFromOwner();
            owner = null;
        }


        /* ~ 生命周期 ~ */
        public virtual void OnConstruct()
        {
        }

        public virtual void OnAddToOwner()
        {
        }

        public virtual void OnRemoveFromOwner()
        {
        }

        public virtual void OnDestory()
        {
        }


        /* ~ 数据保存 ~ */
        public bool Dirty
        {
            get => dirty;
            set => dirty = value;
        }


        public Component(ComponentType<O> type)
        {
            this.type = type;
        }

        public readonly ComponentType<O> type;

        private O? owner;

        private bool dirty;
    }
}
