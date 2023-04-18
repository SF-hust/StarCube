using System;
using System.Numerics;
using System.Diagnostics.CodeAnalysis;

using Newtonsoft.Json.Linq;

using LiteDB;

using StarCube.Utility;
using StarCube.Core.Components;
using StarCube.Game.Levels;

namespace StarCube.Game.Entities.Components
{
    public sealed class LevelComponent : Component<Entity>
    {
        public sealed class Type : EntityComponentType<LevelComponent>
        {
            protected override bool TryCreateFactoryFrom(JObject json, [NotNullWhen(true)] out Func<LevelComponent>? factory)
            {
                factory = () => new LevelComponent();
                return true;
            }

            protected override void StoreTo(LevelComponent component, BsonDocument bson)
            {
                if (component.level != null)
                {
                    bson.Add("guid", component.level.guid);
                }
                bson.Add("pos", component.position);
                bson.Add("rot", component.rotation);
            }

            protected override bool TryRestoreFrom(BsonDocument bson, [NotNullWhen(true)] out LevelComponent? component)
            {
                component = null;

                if (!bson.TryGetGuid("guid", out Guid guid))
                {
                    return false;
                }
                if (!bson.TryGetVector3("pos", out Vector3 position))
                {
                    return false;
                }
                if (!bson.TryGetQuaternion("rot", out Quaternion rotation))
                {
                    return false;
                }

                component = new LevelComponent();
                component.levelGuid = guid;
                component.position = position;
                component.rotation = rotation;
                return true;
            }

            public Type() : base(StringID.Create(Constants.DEFAULT_NAMESPACE, "level"))
            {
            }
        }

        public Level Level
        {
            get => level ?? throw new NullReferenceException();
            set => level ??= value;
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

        public Guid LevelGuid => levelGuid;

        private Level? level = null;

        private Guid levelGuid = Guid.Empty;

        private Vector3 position = Vector3.Zero;

        private Quaternion rotation = Quaternion.Identity;

        public LevelComponent() : base(BuiltinEntityComponentTypes.Level)
        {
        }
    }
}
