using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using Newtonsoft.Json.Linq;

using LiteDB;

using StarCube.Utility;
using StarCube.Core.Registries;

namespace StarCube.Core.Components
{
    public static class ComponentType
    {
        public const string IDName = "cid";
    }

    public abstract class ComponentType<O> : RegistryEntry<ComponentType<O>>
        where O : class, IComponentOwner<O>
    {
        public abstract bool TryCreateComponentFactoryFrom(JObject json, [NotNullWhen(true)] out Func<Component<O>>? factory);

        public abstract void StoreComponentTo(Component<O> component, BsonDocument bson);

        public abstract bool TryRestoreComponentFrom(BsonDocument bson, [NotNullWhen(true)] out Component<O>? component);

        public ComponentType(Registry<ComponentType<O>> registry, StringID id) : base(registry, id)
        {
        }
    }

    public abstract class ComponentType<O, C> : ComponentType<O>
        where O : class, IComponentOwner<O>
        where C : Component<O>
    {
        public sealed override bool TryCreateComponentFactoryFrom(JObject json, [NotNullWhen(true)] out Func<Component<O>>? factory)
        {
            bool result = TryCreateFactoryFrom(json, out Func<C>? fac);
            factory = fac;
            return result;
        }

        public sealed override void StoreComponentTo(Component<O> component, BsonDocument bson)
        {
            Debug.Assert(component is C);
            bson.Add(ComponentType.IDName, ID.idString);
            StoreTo((C)component, bson);
        }

        public sealed override bool TryRestoreComponentFrom(BsonDocument bson, [NotNullWhen(true)] out Component<O>? component)
        {
            bool result = TryRestoreFrom(bson, out C? comp);
            component = comp;
            return result;
        }

        protected abstract bool TryCreateFactoryFrom(JObject json, [NotNullWhen(true)] out Func<C>? factory);

        protected abstract void StoreTo(C component, BsonDocument bson);

        protected abstract bool TryRestoreFrom(BsonDocument bson, [NotNullWhen(true)] out C? component);

        public ComponentType(Registry<ComponentType<O>> registry, StringID id) : base(registry, id)
        {
        }
    }
}
