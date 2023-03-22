using StarCube.Utility.Math;

namespace StarCube.Game.Level.Loading
{
    public abstract class LevelLoadAnchor
    {
        public abstract bool Enable { get; }

        public abstract bool Static { get; }

        public abstract int Radius { get; }

        public abstract ChunkPos GetChunkPos(WorldLevel level);


    }
}
