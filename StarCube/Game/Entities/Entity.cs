using System;

using StarCube.Utility;
using StarCube.Core.Components;
using System.Numerics;

namespace StarCube.Game.Entities
{
    public class Entity :
        IComponentOwner<Entity>,
        IGuid
    {
        Guid IGuid.Guid => guid;

        public Vector3 Position { get => position; set => position = value; }

        public Quaternion Rotation { get => rotation; set => rotation = value; }

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

        private Vector3 position;

        private Quaternion rotation;

        public readonly ComponentContainer<Entity> components;
    }
}
