using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using StarCube.Utility.Math;

namespace StarCube.Game.Level.Chunk
{
    public interface IChunkSource
    {
        public bool TryGetChunkSync(ChunkPos pos, [NotNullWhen(true)] out IChunk? chunk);

        public bool TryGetChunkSync(int x, int y, int z, [NotNullWhen(true)] out IChunk? chunk)
        {
            return TryGetChunkSync(new ChunkPos(x, y, z), out chunk);
        }

        public bool ExistSync(ChunkPos pos);

        public bool ExistSync(int x, int y, int z)
        {
            return ExistSync(new ChunkPos(x, y, z));
        }


        public Task<IChunk?> GetChunkAsync(ChunkPos pos);

        public Task<IChunk?> GetChunkAsync(int x, int y, int z)
        {
            return GetChunkAsync(new ChunkPos(x, y, z));
        }

        public Task<bool> ExistAsync(ChunkPos pos);

        public Task<bool> ExistAsync(int x, int y, int z)
        {
            return ExistAsync(new ChunkPos(x, y, z));
        }
    }
}
