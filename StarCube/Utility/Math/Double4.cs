using System;

namespace StarCube.Utility.Math
{
    public struct Double4 : IEquatable<Double4>
    {
        public double x;
        public double y;
        public double z;
        public double w;

        public Double4(double x, double y, double z, double w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public bool Equals(Double4 other)
        {
            return x == other.x && y == other.y && z == other.z && w == other.w;
        }
    }
}
