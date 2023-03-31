using System;
using System.Collections.Generic;

using StarCube.Utility.Math;

namespace StarCube.Game.Levels.Loading
{
    public struct AnchorData : IEquatable<AnchorData>
    {
        /// <summary>
        /// 获取此数据对应的 Anchor 在指定半径上会加载的 chunk 的坐标
        /// </summary>
        /// <param name="r"></param>
        /// <param name="positions"></param>
        public void GetLoadChunkPos(int r, List<ChunkPos> positions)
        {
            if (r == 0)
            {
                positions.Add(chunkPos);
                return;
            }

            // 前后
            for (int x = (chunkPos.x - r); x <= (chunkPos.x + r); ++x)
            {
                for (int y = (chunkPos.y - r); y <= (chunkPos.y + r); ++y)

                {
                    positions.Add(new ChunkPos(x, y, chunkPos.z + r));
                    positions.Add(new ChunkPos(x, y, chunkPos.z - r));
                }
            }
            // 左右
            for (int z = (chunkPos.z - r + 1); z <= (chunkPos.z + r - 1); ++z)
            {
                for (int y = (chunkPos.y - r); y <= (chunkPos.y + r); ++y)
                {
                    positions.Add(new ChunkPos(chunkPos.x + r, y, z));
                    positions.Add(new ChunkPos(chunkPos.x - r, y, z));
                }
            }
            // 上下
            for (int x = (chunkPos.x - r + 1); x <= (chunkPos.x + r - 1); ++x)
            {
                for (int z = (chunkPos.z - r + 1); z <= (chunkPos.z + r - 1); ++z)
                {
                    positions.Add(new ChunkPos(x, chunkPos.y + r, z));
                    positions.Add(new ChunkPos(x, chunkPos.y - r, z));
                }
            }
        }

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

        public void SetPos(ChunkPos pos)
        {
            anchorData.chunkPos = pos;
        }

        public void SetRadius(int radius)
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
