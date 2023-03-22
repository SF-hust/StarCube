using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using StarCube.Utility.Math;

namespace StarCube.Game.Level.Chunk.Source
{
    public abstract class ChunkSource
    {
        public abstract LevelChunk GetChunkSync(ChunkPos pos);

        public LevelChunk GetChunkSync(int x, int y, int z)
        {
            return GetChunkSync(new ChunkPos(x, y, z));
        }

        public abstract Task<LevelChunk> GetChunkAsync(ChunkPos pos);

        public Task<LevelChunk> GetChunkAsync(int x, int y, int z)
        {
            return GetChunkAsync(new ChunkPos(x, y, z));
        }
    }
}
