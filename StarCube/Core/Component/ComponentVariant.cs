using LiteDB;

using Newtonsoft.Json.Linq;

using StarCube.Utility;

namespace StarCube.Core.Component
{
    public abstract class ComponentVariant<O> : IStringID
        where O : class, IComponentHolder<O>
    {
        StringID IStringID.ID => id;

        public abstract bool TryCreate(JObject args, out Component<O> component);

        public abstract bool RestoreFrom(BsonDocument bson, out Component<O> component);

        public ComponentVariant(ComponentType<O> type, StringID id)
        {
            this.type = type;
            this.id = id;
        }

        public readonly ComponentType<O> type;

        public readonly StringID id;
    }

    public abstract class ComponentVariant<O, C> : ComponentVariant<O>
        where O : class, IComponentHolder<O>
        where C : Component<O>
    {
        public abstract C CreateDefault();

        public abstract bool TryCreate(JObject args, out C component);

        public override bool TryCreate(JObject args, out Component<O> component)
        {
            bool result = TryCreate(args, out C comp);
            component = comp;
            return result;
        }

        public abstract bool RestoreFrom(BsonDocument bson, out C component);

        public override bool RestoreFrom(BsonDocument bson, out Component<O> component)
        {
            bool result = RestoreFrom(bson, out C comp);
            component = comp;
            return result;
        }

        public ComponentVariant(ComponentType<O, C> type, StringID id)
            : base(type, id)
        {
        }
    }
}
