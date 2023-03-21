using System.Numerics;

using LiteDB;

using StarCube.Utility;
using StarCube.BootStrap.Attributes;
using StarCube.Core.Component;

namespace StarCube.Game.Entity.Components
{
    [ConstructInBootStrap]
    public sealed class TransformComponent : Component<Entity>
    {
        public static readonly ComponentType<Entity, TransformComponent> COMPONENT_TYPE =
            new ComponentType<Entity, TransformComponent>(StringID.Create(Constants.DEFAULT_NAMESPACE, "transform"));

        public static readonly ComponentVariant<Entity, TransformComponent> COMPONENT_VARIANT = COMPONENT_TYPE.Register(StringID.Create(Constants.DEFAULT_NAMESPACE, "transform"), TryCreate);

        public static bool TryCreate(BsonDocument bson, out TransformComponent component)
        {
            if (!bson.TryGetVector3("position", out Vector3 position))
            {
                position = Vector3.Zero;
            }
            if (!bson.TryGetQuaternion("rotation", out Quaternion rotation))
            {
                rotation = Quaternion.Identity;
            }

            component = new TransformComponent(position, rotation);
            return true;
        }

        public override ComponentVariant Variant => COMPONENT_VARIANT;

        public override void StoreTo(BsonDocument bson)
        {
            bson.Add("pos", position);
            bson.Add("rot", rotation);
        }

        public override bool RestoreFrom(BsonDocument bson)
        {
            bool result = true;

            if(!bson.TryGetVector3("pos", out position))
            {
                position = Vector3.Zero;
                result = false;
            }
            if(!bson.TryGetQuaternion("rot", out rotation))
            {
                rotation = Quaternion.Identity;
                result = false;
            }

            return result;
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
