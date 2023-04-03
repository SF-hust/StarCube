using System;

using StarCube.Utility;
using StarCube.Core.Components;

namespace StarCube.Game.Entities
{
    public class Entity :
        IComponentHolder<Entity>,
        IGuid
    {
        Guid IGuid.Guid => guid;

        public ComponentContainer<Entity> Components => throw new NotImplementedException();

        public Entity(EntityType type, Guid guid)
        {
            this.type = type;
            this.guid = guid;
        }

        public readonly EntityType type;

        public readonly Guid guid;
    }
}
