using System;
using System.Diagnostics.CodeAnalysis;

using Newtonsoft.Json.Linq;

using LiteDB;

using StarCube.Utility;
using StarCube.Core.Registries;

namespace StarCube.Core.Component
{
    public abstract class Component<O>
        where O : class, IComponentHolder<O>
    {
        /* ~ Type 与 Variant ~ */
        public ComponentType<O> Type => Variant.type;

        public abstract ComponentVariant<O> Variant { get; }



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
        }

        public void ResetOwner()
        {
            if (owner == null)
            {
                return;
            }
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
        public bool Dirty => dirty;

        public void MarkDirty()
        {
            dirty = true;
        }

        public virtual void StoreTo(BsonDocument bson)
        {
            dirty = false;
        }

        public abstract Component<O> Clone();



        public Component()
        {
            owner = null;
            dirty = false;
        }

        private O? owner;

        private bool dirty;
    }

    public static class ComponentExtension
    {
        public static bool TryCreateComponent<O>(this Registry<ComponentType<O>> registry, StringID typeID, StringID variantID, JObject args, [NotNullWhen(true)] out Component<O>? component)
            where O : class, IComponentHolder<O>
        {
            component = null;

            if(!registry.TryGetRegistryEntry(typeID, out ComponentType<O>? type))
            {
                return false;
            }

            if(!type.TryGetVariant(variantID, out ComponentVariant<O>? variant))
            {
                return false;
            }

            return variant.TryCreate(args, out component);
        }
    }
}
