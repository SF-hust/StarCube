using System.Numerics;

using LiteDB;

using Newtonsoft.Json.Linq;

using StarCube.Utility;
using StarCube.Core.Component;
using StarCube.Core.Component.Attributes;

namespace StarCube.Game.Entity.Components
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
                Vector3 position = Vector3.Zero;
                if (args.TryGetArray("position", out JArray? positionArray) &&
                    positionArray.ToFloatArray(out float[] positions) &&
                    positions.Length == 3)
                {
                    position = new Vector3(positions[0], positions[1], positions[2]);
                }

                Quaternion rotation = Quaternion.Identity;
                if (args.TryGetArray("rotation", out JArray? rotationArray) &&
                    rotationArray.ToFloatArray(out float[] rotations) &&
                    rotations.Length == 3)
                {
                    rotation = Quaternion.CreateFromYawPitchRoll(rotations[0], rotations[1], rotations[2]);
                }

                component = new TransformComponent(position, rotation);
                return true;
            }

            public override bool Deserialize(BsonDocument bson, out TransformComponent component)
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

        public TransformComponent(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }


        public Vector3 position;

        public Quaternion rotation;
    }
}
