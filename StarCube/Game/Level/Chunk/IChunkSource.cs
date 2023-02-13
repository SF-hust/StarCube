using System.Diagnostics.CodeAnalysis;

using StarCube.Utility.Math;

namespace StarCube.Game.Level.Chunk
{
    public interface IChunkSource
    {
        public bool Get(int x, int y, int z, [NotNullWhen(true)] out IChunk? chunk);
        public bool Get(ChunkPos pos, [NotNullWhen(true)] out IChunk? chunk)
        {
            return Get(pos.x, pos.y, pos.z, out chunk);
        }

        public bool Exist(int x, int y, int z);
        public bool Exist(ChunkPos pos)
        {
            return Exist(pos.x, pos.y, pos.z);
        }
    }

    public interface IWritableChunkSource : IChunkSource
    {
        public void Set(int x, int y, int z, IChunk chunk);
        public void Set(ChunkPos pos, IChunk chunk)
        {
            Set(pos.x, pos.y, pos.z, chunk);
        }

        public bool Remove(int x, int y, int z);
        public bool Remove(ChunkPos pos)
        {
            return Remove(pos.x, pos.y, pos.z);
        }
    }
}
