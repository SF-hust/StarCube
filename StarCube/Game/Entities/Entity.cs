using System;

using StarCube.Utility;
using StarCube.Core.Components;

namespace StarCube.Game.Entities
{
    public class Entity :
        IComponentOwner<Entity>,
        IGuid
    {
        Guid IGuid.Guid => guid;

        public ComponentContainer<Entity> Components => throw new NotImplementedException();

        public Entity(EntityType type, Guid guid)
        {
            this.guid = guid;
            this.type = type;
            components = new ComponentContainer<Entity>(this);
        }

        public readonly Guid guid;

        public readonly EntityType type;

        public readonly ComponentContainer<Entity> components;
    }
}
