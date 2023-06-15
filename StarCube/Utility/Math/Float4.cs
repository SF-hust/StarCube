using System;

namespace StarCube.Utility.Math
{
    public struct Float4 : IEquatable<Float4>
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public Float4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public bool Equals(Float4 other)
        {
            return x == other.x && y == other.y && z == other.z && w == other.w;
        }
    }
}
