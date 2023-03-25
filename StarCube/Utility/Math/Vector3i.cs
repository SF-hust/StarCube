using System;
using System.Diagnostics.CodeAnalysis;

namespace StarCube.Utility.Math
{
    public readonly struct Vector3i : IEquatable<Vector3i>
    {
        public readonly int x;
        public readonly int y;
        public readonly int z;

        public Vector3i(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static readonly Vector3i Zero = new Vector3i(0, 0, 0);

        public Vector3i SetX(int newX) => new Vector3i(newX, y, z);
        public Vector3i SetY(int newY) => new Vector3i(x, newY, z);
        public Vector3i SetZ(int newZ) => new Vector3i(x, y, newZ);

        public Vector3i Up => new Vector3i(x, y + 1, z);
        public Vector3i Down => new Vector3i(x, y - 1, z);
        public Vector3i North => new Vector3i(x, y, z + 1);
        public Vector3i East => new Vector3i(x + 1, y, z);
        public Vector3i South => new Vector3i(x, y, z - 1);
        public Vector3i West => new Vector3i(x - 1, y, z);

        public Vector3i Add(int a) => new Vector3i(x + a, y + a, z + a);
        public Vector3i Add(Vector3i v) => new Vector3i(x + v.x, y + v.y, z + v.z);
        public Vector3i Mul(int a) => new Vector3i(x * a, y * a, z * a);
        public Vector3i Mul(Vector3i v) => new Vector3i(x * v.x, y * v.y, z * v.z);

        public override int GetHashCode()
        {
            return x * 31 * 31 + y + z * 31;
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is Vector3i other)
            {
                return Equals(other);
            }
            return false;
        }

        public bool Equals(Vector3i other)
        {
            return x == other.x && y == other.y && z == other.z;
        }

        public static bool operator ==(Vector3i left, Vector3i right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector3i left, Vector3i right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return $"{x}, {y}, {z}";
        }
    }

    public static class Vector3iExtensions
    {
        public static BlockPos ToBlockPos(this Vector3i v)
        {
            return new BlockPos(v.x, v.y, v.z);
        }

        public static ChunkPos ToChunkPos(this Vector3i v)
        {
            return new ChunkPos(v.x, v.y, v.z);
        }

        public static RegionPos ToRegionPos(this Vector3i v)
        {
            return new RegionPos(v.x, v.y, v.z);
        }
    }
}
