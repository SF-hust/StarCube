using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

using LiteDB;

using Newtonsoft.Json.Linq;

using StarCube.Utility;
using StarCube.Core.Components;
using StarCube.Core.Components.Attributes;

namespace StarCube.Game.Entities.Components
{
    [DisallowMultipleComponent]
    public sealed class TransformComponent : Component<Entity>
    {
        public sealed class Type : EntityComponentType<TransformComponent>
        {
            protected override bool TryCreateFactoryFrom(JObject json, [NotNullWhen(true)] out Func<TransformComponent>? factory)
            {
                factory = () => new TransformComponent(Vector3.Zero, Quaternion.Identity);
                return true;
            }

            protected override void StoreTo(TransformComponent component, BsonDocument bson)
            {
                bson.Add("pos", component.position);
                bson.Add("rot", component.rotation);
            }

            protected override bool TryRestoreFrom(BsonDocument bson, [NotNullWhen(true)] out TransformComponent? component)
            {
                component = null;

                if (!bson.TryGetVector3("pos", out Vector3 position))
                {
                    return false;
                }
                if (!bson.TryGetQuaternion("pos", out Quaternion rotation))
                {
                    return false;
                }
                component = new TransformComponent(position, rotation);
                return true;
            }

            public Type() : base(StringID.Create(Constants.DEFAULT_NAMESPACE, "transform"))
            {
            }
        }

        public Vector3 Position
        {
            get => position;
            set
            {
                position = value;
                Dirty = true;
            }
        }

        public Quaternion Rotation
        {
            get => rotation;
            set
            {
                rotation = value;
                Dirty = true;
            }
        }

        public TransformComponent(Vector3 position, Quaternion rotation)
            : base(BuiltinEntityComponentTypes.Transform)
        {
            this.position = position;
            this.rotation = rotation;
        }

        private Vector3 position;

        private Quaternion rotation;
    }
}
