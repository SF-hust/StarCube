﻿using StarCube.Utility.Math;

namespace StarCube.Game.Levels
{
    public interface ILevelBound
    {
        public bool InRange(int x, int y, int z);

        public bool InRange(ChunkPos pos);
    }

    public class RectLevelBound : ILevelBound
    {
        public bool InRange(int x, int y, int z)
        {
            return y >= yMin && y < yMax &&
                x < radiusHorizontal && x >= -radiusHorizontal &&
                z < radiusHorizontal && z >= -radiusHorizontal;
        }

        public bool InRange(ChunkPos pos)
        {
            return InRange(pos.x, pos.y, pos.z);
        }

        public RectLevelBound(int yMin, int height, int radiusHorizontal)
        {
            this.yMin = yMin;
            yMax = yMin + height;
            this.radiusHorizontal = radiusHorizontal;
        }

        private readonly int yMin;
        private readonly int yMax;
        private readonly int radiusHorizontal;
    }
}
