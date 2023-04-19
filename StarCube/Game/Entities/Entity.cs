using System;

using StarCube.Utility;
using StarCube.Core.Components;
using StarCube.Game.Ticking;

namespace StarCube.Game.Entities
{
    public class Entity :
        IComponentOwner<Entity>,
        IGuid,
        ITickable
    {
        Guid IGuid.Guid => guid;

        public ComponentContainer<Entity> Components => throw new NotImplementedException();

        public void Tick()
        {
        }

        public Entity(EntityType type, Guid guid, bool standalone)
        {
            this.guid = guid;
            this.type = type;
            this.standalone = standalone;
            components = new ComponentContainer<Entity>(this);
        }

        public readonly Guid guid;

        public readonly EntityType type;

        public readonly bool standalone;

        public readonly ComponentContainer<Entity> components;

    }
}
