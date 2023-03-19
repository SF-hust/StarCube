using System;
using System.Diagnostics;

using LiteDB;

namespace StarCube.Core.Component
{
    public abstract class Component<O> : IComponent<O>
        where O : class, IComponentHolder<O>
    {
        public Type OwnerType => typeof(O);


        public abstract ComponentVariant Variant { get; }


        public bool HasOwner => owner != null;

        public O Owner => owner ?? throw new NullReferenceException();


        public bool Modified => modified;

        public void Modify()
        {
            modified = true;
        }

        public virtual void StoreTo(BsonDocument bosn)
        {
            modified = false;
        }

        public virtual bool RestoreFrom(BsonDocument bosn)
        {
            return true;
        }


        public virtual void OnAddToOwner(O newOwner)
        {
            if (newOwner == owner)
            {
                return;
            }
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



        public Component()
        {
        }

        private O? owner = null;

        private bool modified = false;
    }
}
