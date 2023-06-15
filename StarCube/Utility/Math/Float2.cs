using System;

namespace StarCube.Utility.Math
{
    public struct Float2 : IEquatable<Float2>
    {
        public float x;
        public float y;

        public Float2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public bool Equals(Float2 other)
        {
            return x == other.x && y == other.y;
        }
    }
}
