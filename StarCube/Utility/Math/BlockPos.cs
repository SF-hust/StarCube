using System;
using System.Diagnostics.CodeAnalysis;

using StarCube.Utility.Enums;

namespace StarCube.Utility.Math
{
    public readonly struct BlockPos : IEquatable<BlockPos>
    {
        public static int InChunkPosToIndex(int x, int y, int z)
        {
            return (y << 8) + (z << 4) + x;
        }


        public readonly int x, y, z;
        public BlockPos(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static readonly BlockPos Zero = new BlockPos(0, 0, 0);

        public Vector3i ToVector3i => new Vector3i(x, y, z);

        public BlockPos InChunkPos => new BlockPos(x & 0xF, y & 0xF, z & 0xF);

        public ChunkPos ChunkPos => new ChunkPos(x >> 4, y >> 4, z >> 4);

        public BlockPos SetX(int newX) => new BlockPos(newX, y, z);
        public BlockPos SetY(int newY) => new BlockPos(x, newY, z);
        public BlockPos SetZ(int newZ) => new BlockPos(x, y, newZ);
        public BlockPos SetAxis(Axis axis, int newValue)
        {
            return axis switch
            {
                Axis.X => new BlockPos(newValue, y, z),
                Axis.Y => new BlockPos(x, newValue, z),
                Axis.Z => new BlockPos(x, y, newValue),
                _ => throw new Exception("BlockPos : axis is illegal"),
            };
        }

        public BlockPos Up => new BlockPos(x, y + 1, z);
        public BlockPos Down => new BlockPos(x, y - 1, z);
        public BlockPos North => new BlockPos(x, y, z + 1);
        public BlockPos East => new BlockPos(x + 1, y, z);
        public BlockPos South => new BlockPos(x, y, z - 1);
        public BlockPos West => new BlockPos(x - 1, y, z);
        public BlockPos Move(Axis axis, int offset)
        {
            return axis switch
            {
                Axis.X => new BlockPos(x + offset, y, z),
                Axis.Y => new BlockPos(x, y + offset, z),
                Axis.Z => new BlockPos(x, y, z + offset),
                _ => throw new Exception("BlockPos : axis is illegal"),
            };
        }
        public BlockPos Move(Direction direction, int offset)
        {
            return direction switch
            {
                Direction.Up => new BlockPos(x, y + offset, z),
                Direction.Down => new BlockPos(x, y - offset, z),
                Direction.North => new BlockPos(x, y, z + offset),
                Direction.East => new BlockPos(x + offset, y, z),
                Direction.South => new BlockPos(x, y, z - offset),
                Direction.West => new BlockPos(x - offset, y, z),
                _ => throw new Exception("BlockPos : direction is illegal"),
            };
        }


        public override int GetHashCode()
        {
            return x * 953 + y + z * 31;
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is BlockPos other)
            {
                return Equals(other);
            }
            return false;
        }

        public bool Equals(BlockPos other)
        {
            return x == other.x && y == other.y && z == other.z;
        }

        public static bool operator ==(BlockPos left, BlockPos right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BlockPos left, BlockPos right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return $"{x}, {y}, {z}";
        }
    }
}
