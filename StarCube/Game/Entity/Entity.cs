using System;

using StarCube.Utility;
using StarCube.Core.Component;

namespace StarCube.Game.Entity
{
    public class Entity :
        IComponentHolder<Entity>,
        IGuid
    {
        public Guid Guid => guid;

        public ComponentHolder<Entity> Components => throw new NotImplementedException();

        public Entity(Guid guid)
        {
            this.guid = guid;
        }

        private readonly Guid guid;
    }
}
