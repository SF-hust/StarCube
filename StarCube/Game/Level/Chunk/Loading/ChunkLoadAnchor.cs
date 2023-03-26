using System;
using StarCube.Utility.Math;

namespace StarCube.Game.Level.Loading
{
    public struct AnchorData : IEquatable<AnchorData>
    {
        public static bool operator ==(AnchorData left, AnchorData right)
        {
            return left.chunkPos == right.chunkPos && left.radius == right.radius;
        }

        public static bool operator !=(AnchorData left, AnchorData right)
        {
            return left.chunkPos != right.chunkPos || left.radius != right.radius;
        }

        public override bool Equals(object? obj)
        {
            return obj is AnchorData data && Equals(data);
        }

        public bool Equals(AnchorData other)
        {
            return chunkPos == other.chunkPos && radius == other.radius;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(chunkPos, radius);
        }

        public AnchorData(ChunkPos chunkPos, int radius)
        {
            this.chunkPos = chunkPos;
            this.radius = radius;
        }

        public ChunkPos chunkPos;
        public int radius;
    }

    public sealed class ChunkLoadAnchor
    {
        public ChunkPos ChunkPos => anchorData.chunkPos;
        public int Radius => anchorData.radius;

        public AnchorData Current => anchorData;

        public void UpdatePos(ChunkPos pos)
        {
            anchorData.chunkPos = pos;
        }
        public void UpdateRadius(int radius)
        {
            anchorData.radius = radius;
        }


        public ChunkLoadAnchor(int radius)
        {
            anchorData = new AnchorData(ChunkPos.Zero, radius);
        }

        private AnchorData anchorData;
    }
}
