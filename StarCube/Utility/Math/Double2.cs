using System;

namespace StarCube.Utility.Math
{
    public struct Double2 : IEquatable<Double2>
    {
        public double x;
        public double y;

        public Double2(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public bool Equals(Double2 other)
        {
            return x == other.x && y == other.y;
        }
    }
}
