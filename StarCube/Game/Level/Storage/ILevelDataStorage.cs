using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using StarCube.Utility.Math;
using StarCube.Game.Level.Chunk;

namespace StarCube.Game.Level.Storage
{
    public interface ILevelDataStorage
    {
        public bool Contains(ChunkPos pos);

        public Task<LevelChunk?> ReadChunk(ChunkPos pos);

        public bool ReadChunkSync(ChunkPos pos, [NotNullWhen(true)] out LevelChunk? chunk);

        public void WriteChunk(LevelChunk chunk);
    }
}
