using System;
using System.Diagnostics.CodeAnalysis;

using StarCube.Utility.Enums;

namespace StarCube.Utility.Math
{
    public readonly struct Vector3i : IEquatable<Vector3i>
    {
        public readonly int x, y, z;
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
        public Vector3i SetAxis(Axis axis, int newValue)
        {
            return axis switch
            {
                Axis.X => new Vector3i(newValue, y, z),
                Axis.Y => new Vector3i(x, newValue, z),
                Axis.Z => new Vector3i(x, y, newValue),
                _ => throw new Exception("BlockPos : axis is illegal"),
            };
        }

        public Vector3i Up => new Vector3i(x, y + 1, z);
        public Vector3i Down => new Vector3i(x, y - 1, z);
        public Vector3i North => new Vector3i(x, y, z + 1);
        public Vector3i East => new Vector3i(x + 1, y, z);
        public Vector3i South => new Vector3i(x, y, z - 1);
        public Vector3i West => new Vector3i(x - 1, y, z);
        public Vector3i Move(Axis axis, int offset)
        {
            return axis switch
            {
                Axis.X => new Vector3i(x + offset, y, z),
                Axis.Y => new Vector3i(x, y + offset, z),
                Axis.Z => new Vector3i(x, y, z + offset),
                _ => throw new Exception("Vector3i : axis is illegal"),
            };
        }
        public Vector3i Move(Direction direction, int offset)
        {
            return direction switch
            {
                Direction.Up => new Vector3i(x, y + offset, z),
                Direction.Down => new Vector3i(x, y - offset, z),
                Direction.North => new Vector3i(x, y, z + offset),
                Direction.East => new Vector3i(x + offset, y, z),
                Direction.South => new Vector3i(x, y, z - offset),
                Direction.West => new Vector3i(x - offset, y, z),
                _ => throw new Exception("Vector3i : direction is illegal"),
            };
        }

        public Vector3i MoveX(int offset) => new Vector3i(x + offset, y, z);
        public Vector3i MoveY(int offset) => new Vector3i(x, y + offset, z);
        public Vector3i MoveZ(int offset) => new Vector3i(x, y, z + offset);

        public Vector3i Add(int a) => new Vector3i(x + a, y + a, z + a);
        public Vector3i Add(Vector3i v) => new Vector3i(x + v.x, y + v.y, z + v.z);
        public Vector3i Mul(int a) => new Vector3i(x * a, y * a, z * a);
        public Vector3i Mul(Vector3i v) => new Vector3i(x * v.x, y * v.y, z * v.z);

        public override int GetHashCode()
        {
            return x + y * 31 + z * 31 * 31;
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
    }
}
