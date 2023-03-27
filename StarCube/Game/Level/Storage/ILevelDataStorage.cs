using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using StarCube.Utility.Math;
using StarCube.Game.Level.Chunks;

namespace StarCube.Game.Level.Storage
{
    public interface ILevelDataStorage
    {
        public bool Contains(ChunkPos pos);

        public Task<Chunk?> ReadChunk(ChunkPos pos);

        public bool ReadChunkSync(ChunkPos pos, [NotNullWhen(true)] out Chunk? chunk);

        public void WriteChunk(Chunk chunk);
    }
}
