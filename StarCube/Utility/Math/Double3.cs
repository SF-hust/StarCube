using System;

namespace StarCube.Utility.Math
{
    public struct Double3 : IEquatable<Double3>
    {
        public double x;
        public double y;
        public double z;

        public Double3(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public bool Equals(Double3 other)
        {
            return x == other.x && y == other.y && z == other.z;
        }
    }
}
