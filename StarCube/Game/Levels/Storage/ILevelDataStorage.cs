using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using StarCube.Utility.Math;
using StarCube.Game.Levels.Chunks;

namespace StarCube.Game.Levels.Storage
{
    public interface ILevelDataStorage
    {
        public bool Contains(ChunkPos pos);

        public Task<Chunk?> ReadChunk(ChunkPos pos);

        public bool ReadChunkSync(ChunkPos pos, [NotNullWhen(true)] out Chunk? chunk);

        public void WriteChunk(Chunk chunk);
    }
}
