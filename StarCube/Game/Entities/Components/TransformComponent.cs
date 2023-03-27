using System.Numerics;

using LiteDB;

using Newtonsoft.Json.Linq;

using StarCube.Utility;
using StarCube.Core.Component;
using StarCube.Core.Component.Attributes;

namespace StarCube.Game.Entities.Components
{
    [ComponentType]
    [ComponentVariant]
    public sealed class TransformComponent : Component<Entity>
    {
        public static readonly StringID ComponentID = StringID.Create(Constants.DEFAULT_NAMESPACE, "transform");

        public static readonly ComponentType<Entity, TransformComponent> COMPONENT_TYPE =
            new ComponentType<Entity, TransformComponent>(ComponentID);

        public static readonly ComponentVariant<Entity, TransformComponent> COMPONENT_VARIANT = new TransformVariant();

        private sealed class TransformVariant : ComponentVariant<Entity, TransformComponent>
        {
            public override TransformComponent CreateDefault()
            {
                return new TransformComponent(Vector3.Zero, Quaternion.Identity);
            }

            public override bool TryCreate(JObject args, out TransformComponent component)
            {
                component = new TransformComponent(Vector3.Zero, Quaternion.Identity);
                return true;
            }

            public override bool RestoreFrom(BsonDocument bson, out TransformComponent component)
            {
                bool result = true;

                if (!bson.TryGetVector3("pos", out Vector3 position))
                {
                    position = Vector3.Zero;
                    result = false;
                }

                if (!bson.TryGetQuaternion("rot", out Quaternion rotation))
                {
                    rotation = Quaternion.Identity;
                    result = false;
                }

                component = new TransformComponent(position, rotation);
                return result;
            }

            public TransformVariant()
                : base(COMPONENT_TYPE, ComponentID)
            {
            }
        }

        public override ComponentVariant<Entity> Variant => COMPONENT_VARIANT;

        public override void StoreTo(BsonDocument bson)
        {
            bson.Add("pos", position);
            bson.Add("rot", rotation);
        }

        public override Component<Entity> Clone()
        {
            return new TransformComponent(position, rotation);
        }

        public TransformComponent(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }


        public Vector3 position;

        public Quaternion rotation;
    }
}
