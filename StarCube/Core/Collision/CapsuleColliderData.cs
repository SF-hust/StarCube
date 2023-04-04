using System.Numerics;

using StarCube.Utility.Enums;

namespace StarCube.Core.Collision
{
    public struct CapsuleColliderData
    {
        public Vector3 center;
        public float radius;
        public float height;
        public Axis axis;
    }
}
