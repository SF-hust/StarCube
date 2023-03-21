using LiteDB;

using StarCube.Utility;

namespace StarCube.Core.Component
{
    public abstract class ComponentVariant<O> : IStringID
        where O : class, IComponentHolder<O>
    {
        StringID IStringID.ID => id;

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
        where C : IComponent<O>
    {
        public abstract C CreateDefault();

        public abstract bool TryCreate(BsonDocument bson, out C component);

        public abstract bool Deserialize(BsonDocument bson, out C component);


        public ComponentVariant(ComponentType<O, C> type, StringID id)
            : base(type, id)
        {
        }
    }
}
