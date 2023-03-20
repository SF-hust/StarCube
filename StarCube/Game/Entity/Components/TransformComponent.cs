using System.Numerics;

using LiteDB;

using StarCube.Utility;
using StarCube.Core.Component;

namespace StarCube.Game.Entity.Components
{
    public sealed class TransformComponent : Component<Entity>
    {
        public static readonly ComponentType<Entity, TransformComponent> COMPONENT_TYPE =
            new ComponentType<Entity, TransformComponent>(StringID.Create(Constants.DEFAULT_NAMESPACE, "transform"));

        public static readonly ComponentVariant<Entity, TransformComponent> COMPONENT_VARIANT = COMPONENT_TYPE.Register(StringID.Create(Constants.DEFAULT_NAMESPACE, "transform"), TryCreate);

        public static bool TryCreate(BsonDocument bson, out TransformComponent component)
        {
            component = new TransformComponent();
            return true;
        }

        public override ComponentVariant Variant => COMPONENT_VARIANT;

        public TransformComponent()
        {

        }


        public readonly Vector3 position;

        public readonly Quaternion rotation;
    }
}
