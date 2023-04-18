using System;

namespace StarCube.Core.Components
{
    public abstract class Component<O>
        where O : class, IComponentOwner<O>
    {
        /* ~ Owner ~ */
        public bool Attached => owner != null;

        public O Owner => owner ?? throw new NullReferenceException(nameof(Owner));

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

        /// <summary>
        /// 当 SetOwner 调用后调用此方法
        /// </summary>
        public virtual void OnAddToOwner()
        {
        }

        /// <summary>
        /// 在 ResetOwner 调用前调用此方法
        /// </summary>
        public virtual void OnRemoveFromOwner()
        {
        }

        /// <summary>
        /// 当组件附加的实例变得活跃时调用此方法
        /// </summary>
        public virtual void OnActive()
        {
        }

        /// <summary>
        /// 当组件附加的实例变得不活跃时调用此方法
        /// </summary>
        public virtual void OnInactive()
        {
        }

        /// <summary>
        /// 当组件被存储时调用此方法
        /// </summary>
        public virtual void OnStore()
        {
        }

        /// <summary>
        /// 当组件从存储中还原时调用此方法
        /// </summary>
        public virtual void OnRestore()
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
