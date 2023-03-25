using System;

using StarCube.Utility.Math;

namespace StarCube.Game.Level.Loading
{
    public class LevelLoadAnchor
    {
        public ChunkPos GetChunkPos()
        {
            return chunkPos;
        }

        public bool InRange(ChunkPos pos)
        {
            int xOff = Math.Abs(chunkPos.x - pos.x);
            int yOff = Math.Abs(chunkPos.y - pos.y);
            int zOff = Math.Abs(chunkPos.z - pos.z);

            return xOff <= radius && yOff <= radius && zOff <= radius;
        }

        public void UpdataPosition(float x, float y, float z)
        {
            chunkPos = new ChunkPos((int)x >> 4, (int)y >> 4, (int)z >> 4);
        }

        public LevelLoadAnchor(int radius)
        {
            this.radius = radius;
        }

        public readonly int radius;

        private ChunkPos chunkPos;
    }
}
