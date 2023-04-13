using System;
using System.Collections.Generic;
using System.Threading;

using StarCube.Utility.Math;

namespace StarCube.Game.Levels.Loading
{
    public readonly struct AnchorData : IEquatable<AnchorData>
    {
        /// <summary>
        /// 获取此数据对应的 Anchor 在指定半径上会加载的 chunk 的坐标
        /// </summary>
        /// <param name="r"></param>
        /// <param name="positions"></param>
        /// <param name="bound"></param>
        public void GetLoadChunkPos(int r, List<ChunkPos> positions, ILevelBound bound)
        {
            if (r == 0)
            {
                if (bound.InRange(chunkPos))
                {
                    positions.Add(chunkPos);
                }
                return;
            }

            // 前后
            for (int x = (chunkPos.x - r); x <= (chunkPos.x + r); ++x)
            {
                for (int y = (chunkPos.y - r); y <= (chunkPos.y + r); ++y)
                {
                    ChunkPos front = new ChunkPos(x, y, chunkPos.z + r);
                    if (bound.InRange(front))
                    {
                        positions.Add(front);
                    }
                    ChunkPos back = new ChunkPos(x, y, chunkPos.z - r);
                    if (bound.InRange(back))
                    {
                        positions.Add(back);
                    }
                }
            }
            // 左右
            for (int z = (chunkPos.z - r + 1); z <= (chunkPos.z + r - 1); ++z)
            {
                for (int y = (chunkPos.y - r); y <= (chunkPos.y + r); ++y)
                {
                    ChunkPos left = new ChunkPos(chunkPos.x - r, y, z);
                    if (bound.InRange(left))
                    {
                        positions.Add(left);
                    }
                    ChunkPos right = new ChunkPos(chunkPos.x + r, y, z);
                    if (bound.InRange(right))
                    {
                        positions.Add(right);
                    }
                }
            }
            // 上下
            for (int x = (chunkPos.x - r + 1); x <= (chunkPos.x + r - 1); ++x)
            {
                for (int z = (chunkPos.z - r + 1); z <= (chunkPos.z + r - 1); ++z)
                {
                    ChunkPos up = new ChunkPos(x, chunkPos.y + r, z);
                    if (bound.InRange(up))
                    {
                        positions.Add(up);
                    }
                    ChunkPos down = new ChunkPos(x, chunkPos.y - r, z);
                    if (bound.InRange(down))
                    {
                        positions.Add(down);
                    }
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

        public readonly ChunkPos chunkPos;
        public readonly int radius;
    }

    /// <summary>
    /// 表示一个可以加载 Level 中 Chunk 的锚点，线程安全
    /// </summary>
    public sealed class ChunkLoadAnchor
    {
        public ChunkPos ChunkPos => Current.chunkPos;

        public int Radius => Current.radius;

        public AnchorData Current
        {
            get
            {
                rwLock.EnterReadLock();
                AnchorData data = anchorData;
                rwLock.ExitReadLock();
                return data;
            }
        }

        public void SetChunkPos(ChunkPos pos)
        {
            rwLock.EnterWriteLock();
            anchorData = new AnchorData(pos, anchorData.radius);
            rwLock.ExitWriteLock();
        }

        public void SetRadius(int radius)
        {
            rwLock.EnterWriteLock();
            anchorData = new AnchorData(anchorData.chunkPos, radius);
            rwLock.ExitWriteLock();
        }

        public void SetPosAndRadius(ChunkPos pos, int radius)
        {
            rwLock.EnterWriteLock();
            anchorData = new AnchorData(pos, radius);
            rwLock.ExitWriteLock();
        }

        public ChunkLoadAnchor(int radius)
        {
            anchorData = new AnchorData(ChunkPos.Zero, radius);
            rwLock = new ReaderWriterLockSlim();
        }

        private AnchorData anchorData;

        private readonly ReaderWriterLockSlim rwLock;
    }
}
