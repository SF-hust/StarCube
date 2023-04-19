using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using LiteDB;
using StarCube.Utility.Enums;

namespace StarCube.Utility.Math
{
    public readonly struct ChunkPos : IEquatable<ChunkPos>
    {
        public static readonly ChunkPos Zero = new ChunkPos(0, 0, 0);

        public static int InRegionPosToIndex(int x, int y, int z)
        {
            return (y << 8) + (z << 4) + x;
        }


        public readonly int x;
        public readonly int y;
        public readonly int z;

        public ChunkPos(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public int ToIndex() => (y << 8) + (z << 4) + x;

        public Vector3i ToVector3i => new Vector3i(x, y, z);

        public ChunkPos SetX(int newX) => new ChunkPos(newX, y, z);
        public ChunkPos SetY(int newY) => new ChunkPos(x, newY, z);
        public ChunkPos SetZ(int newZ) => new ChunkPos(x, y, newZ);
        public ChunkPos SetAxis(Axis axis, int newValue)
        {
            return axis switch
            {
                Axis.X => new ChunkPos(newValue, y, z),
                Axis.Y => new ChunkPos(x, newValue, z),
                Axis.Z => new ChunkPos(x, y, newValue),
                _ => throw new Exception("ChunkPos : axis is illegal"),
            };
        }

        public ChunkPos Up => new ChunkPos(x, y + 1, z);
        public ChunkPos Down => new ChunkPos(x, y - 1, z);
        public ChunkPos North => new ChunkPos(x, y, z + 1);
        public ChunkPos East => new ChunkPos(x + 1, y, z);
        public ChunkPos South => new ChunkPos(x, y, z - 1);
        public ChunkPos West => new ChunkPos(x - 1, y, z);

        public override int GetHashCode()
        {
            return x * 31 * 31 + y + z * 31;
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is ChunkPos other)
            {
                return Equals(other);
            }
            return false;
        }

        public bool Equals(ChunkPos other)
        {
            return x == other.x && y == other.y && z == other.z;
        }

        public static bool operator ==(ChunkPos left, ChunkPos right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ChunkPos left, ChunkPos right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return $"{x}, {y}, {z}";
        }
    }

    public static class ChunkPosExtension
    {
        private static readonly ThreadLocal<byte[]> ThreadLocalBuffer = new ThreadLocal<byte[]>(() => new byte[12]);

        public static RegionPos GetRegionPos(this ChunkPos chunkPos)
        {
            return new RegionPos(chunkPos.x >> 4, chunkPos.y >> 4, chunkPos.z >> 4);
        }

        public static ObjectId ToObjectID(this ChunkPos chunkPos)
        {
            byte[] buffer = ThreadLocalBuffer.Value;
            Span<int> ints = MemoryMarshal.Cast<byte, int>(buffer);
            ints[0] = chunkPos.x;
            ints[1] = chunkPos.y;
            ints[2] = chunkPos.z;
            if (BitConverter.IsLittleEndian)
            {
                buffer.AsSpan(0, 4).Reverse();
                buffer.AsSpan(4, 4).Reverse();
                buffer.AsSpan(8, 4).Reverse();
            }
            return new ObjectId(buffer);
        }

        public static ChunkPos ToChunkPos(this ObjectId id)
        {
            byte[] buffer = ThreadLocalBuffer.Value;
            id.ToByteArray(buffer, 0);
            if (BitConverter.IsLittleEndian)
            {
                buffer.AsSpan(0, 4).Reverse();
                buffer.AsSpan(4, 4).Reverse();
                buffer.AsSpan(8, 4).Reverse();
            }
            ReadOnlySpan<int> ints = MemoryMarshal.Cast<byte, int>(buffer);
            return new ChunkPos(ints[0], ints[1], ints[2]);
        }
    }
}
