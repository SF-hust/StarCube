using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace StarCube.Utility.Math
{
    public readonly struct RegionPos : IEquatable<RegionPos>
    {
        public static readonly RegionPos Zero = new RegionPos(0, 0, 0);


        public readonly int x;
        public readonly int y;
        public readonly int z;

        public RegionPos(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3i ToVector3i => new Vector3i(x, y, z);

        public RegionPos Up => new RegionPos(x, y + 1, z);
        public RegionPos Down => new RegionPos(x, y - 1, z);
        public RegionPos North => new RegionPos(x, y, z + 1);
        public RegionPos East => new RegionPos(x + 1, y, z);
        public RegionPos South => new RegionPos(x, y, z - 1);
        public RegionPos West => new RegionPos(x - 1, y, z);

        public override int GetHashCode()
        {
            return x * 31 * 31 + y + z * 31;
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is RegionPos other)
            {
                return Equals(other);
            }
            return false;
        }

        public bool Equals(RegionPos other)
        {
            return x == other.x && y == other.y && z == other.z;
        }

        public static bool operator ==(RegionPos left, RegionPos right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RegionPos left, RegionPos right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return $"{x}, {y}, {z}";
        }
    }
}
