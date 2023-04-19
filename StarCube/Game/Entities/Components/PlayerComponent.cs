using System;
using System.Diagnostics.CodeAnalysis;

using Newtonsoft.Json.Linq;

using LiteDB;

using StarCube.Utility;
using StarCube.Core.Components;

namespace StarCube.Game.Entities.Components
{
    public sealed class PlayerComponent : Component<Entity>
    {
        public sealed class Type : EntityComponentType<PlayerComponent>
        {
            protected override void StoreTo(PlayerComponent component, BsonDocument bson)
            {
                bson.Add("guid", component.guid);
            }

            protected override bool TryCreateFactoryFrom(JObject json, [NotNullWhen(true)] out Func<PlayerComponent>? factory)
            {
                factory = null;
                return false;
            }

            protected override bool TryRestoreFrom(BsonDocument bson, [NotNullWhen(true)] out PlayerComponent? component)
            {
                if (bson.TryGetGuid("guid", out Guid guid))
                {
                    component = new PlayerComponent(guid);
                    return true;
                }

                component = null;
                return false;
            }

            public Type() : base(StringID.Create(Constants.DEFAULT_NAMESPACE, "player"))
            {
            }
        }

        public PlayerComponent(Guid guid) : base(BuiltinEntityComponentTypes.Player)
        {
            this.guid = guid;
        }

        public readonly Guid guid;
    }
}
