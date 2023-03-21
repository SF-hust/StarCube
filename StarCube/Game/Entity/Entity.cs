using System;

using StarCube.Utility;
using StarCube.Core.Component;

namespace StarCube.Game.Entity
{
    public class Entity :
        IComponentHolder<Entity>,
        IGuid
    {
        Guid IGuid.Guid => guid;

        public ComponentHolder<Entity> Components => throw new NotImplementedException();

        public Entity(EntityType type, Guid guid)
        {
            this.type = type;
            this.guid = guid;
        }

        public readonly EntityType type;

        public readonly Guid guid;
    }
}
