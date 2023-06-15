using System;

namespace StarCube.Utility.Math
{
    public struct Float3 : IEquatable<Float3>
    {
        public float x;
        public float y;
        public float z;

        public Float3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public bool Equals(Float3 other)
        {
            return x == other.x && y == other.y && z == other.z;
        }
    }
}
