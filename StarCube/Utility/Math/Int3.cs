using System;

namespace StarCube.Utility.Math
{
    public struct Int3 : IEquatable<Int3>
    {
        public static readonly Int3 Zero = new Int3(0, 0, 0);

        public int x;
        public int y;
        public int z;

        public Int3(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public bool Equals(Int3 other)
        {
            return x == other.x && y == other.y && z == other.z;
        }
    }
}
