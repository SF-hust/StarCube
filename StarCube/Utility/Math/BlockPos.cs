using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;

using LiteDB;

namespace StarCube.Utility.Math
{
    public readonly struct BlockPos : IEquatable<BlockPos>
    {
        public static readonly BlockPos Zero = new BlockPos(0, 0, 0);

        public static int InChunkPosToIndex(int x, int y, int z)
        {
            return (y << 8) + (z << 4) + x;
        }

        public static BlockPos FromInChunkIndex(int index)
        {
            return new BlockPos(index & 0xF, (index >> 8) & 0xF, (index >> 4) & 0xF);
        }

        public int InChunkIndex => (y << 8) + (z << 4) + x;

        public readonly int x;
        public readonly int y;
        public readonly int z;

        public BlockPos(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// 只对 chunk 中的相对方块坐标有效
        /// </summary>
        public int ToIndex() => (y << 8) + (z << 4) + x;

        public Int3 ToVector3i => new Int3(x, y, z);

        public BlockPos North => new BlockPos(x, y, z + 1);
        public BlockPos South => new BlockPos(x, y, z - 1);
        public BlockPos East => new BlockPos(x + 1, y, z);
        public BlockPos West => new BlockPos(x - 1, y, z);
        public BlockPos Up => new BlockPos(x, y + 1, z);
        public BlockPos Down => new BlockPos(x, y - 1, z);

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
        private static readonly ThreadLocal<byte[]> ThreadLocalBuffer = new ThreadLocal<byte[]>(() => new byte[12]);

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

        public static ObjectId ToObjectID(this BlockPos blockPos)
        {
            byte[] buffer = ThreadLocalBuffer.Value;
            Span<int> ints = MemoryMarshal.Cast<byte, int>(buffer);
            ints[0] = blockPos.x;
            ints[1] = blockPos.y;
            ints[2] = blockPos.z;
            return new ObjectId(buffer);
        }

        public static BlockPos ToBlockPos(this ObjectId id)
        {
            byte[] buffer = ThreadLocalBuffer.Value;
            id.ToByteArray(buffer, 0);
            ReadOnlySpan<int> ints = MemoryMarshal.Cast<byte, int>(buffer);
            return new BlockPos(ints[0], ints[1], ints[2]);
        }
    }
}
