using LiteDB;

using StarCube.Utility;

namespace StarCube.Core.Component
{
    public delegate bool ComponentFactory<O, C>(BsonDocument bson, out C component)
        where O : class, IComponentHolder<O>
        where C : IComponent<O>;

    public abstract class ComponentVariant
    {
        public readonly ComponentType type;

        public readonly StringID id;

        public ComponentVariant(ComponentType type, StringID id)
        {
            this.type = type;
            this.id = id;
        }
    }

    public class ComponentVariant<O, C> : ComponentVariant
        where O : class, IComponentHolder<O>
        where C : IComponent<O>
    {
        public C CreateDefault()
        {
            return Create(new BsonDocument());
        }

        public C Create(BsonDocument bson)
        {
            tryCreateComponent(bson, out C component);
            return component;
        }


        public ComponentVariant(ComponentType<O, C> type, StringID id, ComponentFactory<O, C> factory)
            : base(type, id)
        {
            tryCreateComponent = factory;
        }

        private readonly ComponentFactory<O, C> tryCreateComponent;
    }
}
