using System;
using System.Diagnostics.CodeAnalysis;

namespace StarCube.Utility.Math
{
    public readonly struct BlockPos : IEquatable<BlockPos>
    {
        public static readonly BlockPos Zero = new BlockPos(0, 0, 0);

        public static int InChunkPosToIndex(int x, int y, int z)
        {
            return (y << 8) + (z << 4) + x;
        }


        public readonly int x;
        public readonly int y;
        public readonly int z;

        public BlockPos(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }


        public Vector3i ToVector3i => new Vector3i(x, y, z);

        public BlockPos Up => new BlockPos(x, y + 1, z);
        public BlockPos Down => new BlockPos(x, y - 1, z);
        public BlockPos North => new BlockPos(x, y, z + 1);
        public BlockPos East => new BlockPos(x + 1, y, z);
        public BlockPos South => new BlockPos(x, y, z - 1);
        public BlockPos West => new BlockPos(x - 1, y, z);

        public override int GetHashCode()
        {
            return x * 31 * 31 + y + z * 31;
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

    public static class BlockPosExtention
    {
        public static ChunkPos GetChunkPos(this BlockPos blockPos)
        {
            return new ChunkPos(blockPos.x >> 4, blockPos.y >> 4, blockPos.z >> 4);
        }

        public static BlockPos GetInChunkPos(this BlockPos blockPos)
        {
            return new BlockPos(blockPos.x & 0xF, blockPos.y & 0xF, blockPos.z & 0xF);
        }

        public static RegionPos GetRegionPos(this BlockPos blockPos)
        {
            return new RegionPos(blockPos.x >> 8, blockPos.y >> 8, blockPos.z >> 8);
        }
    }
}
