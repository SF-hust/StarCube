using System.Diagnostics.CodeAnalysis;

using StarCube.Utility.Math;
using StarCube.Game.Levels.Chunks;

namespace StarCube.Game.Levels.Storage
{
    public class RegionMap
    {

    }

    public class LevelDataStorage
    {
        public bool Contains(ChunkPos pos)
        {
            return false;
        }

        public bool TryLoadChunk(ChunkPos pos, [NotNullWhen(true)] out Chunk? chunk)
        {
            chunk = null;
            return false;
        }

        public void WriteChunk(Chunk chunk)
        {
        }
    }
}
