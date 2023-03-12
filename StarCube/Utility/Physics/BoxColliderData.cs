using System.Numerics;

namespace StarCube.Utility.Physics
{
    public struct BoxColliderData
    {
        public static readonly BoxColliderData FULLCUBE = new BoxColliderData { center = new Vector3(0.5f, 0.5f, 0.5f), size = new Vector3(1.0f, 1.0f, 1.0f)};

        public Vector3 center;
        public Vector3 size;
    }
}
