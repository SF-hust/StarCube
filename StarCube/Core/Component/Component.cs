using System;

using LiteDB;

namespace StarCube.Core.Component
{
    public abstract class Component<O> : IComponent<O>
        where O : class, IComponentHolder<O>
    {
        public Type OwnerType => typeof(O);


        public abstract ComponentVariant<O> Variant { get; }


        public bool HasOwner => owner != null;

        public O Owner => owner ?? throw new NullReferenceException();


        public bool Dirty => dirty;

        public void MarkDirty()
        {
            dirty = true;
        }

        public virtual void Serialize(BsonDocument bson)
        {
            dirty = false;
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

        private bool dirty = false;
    }
}
