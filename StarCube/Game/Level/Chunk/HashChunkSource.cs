using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using StarCube.Utility.Math;

namespace StarCube.Game.Level.Chunk
{
    public class HashChunkSource : IWritableChunkSource
    {
        private readonly Dictionary<ChunkPos, IChunk> chunks = new Dictionary<ChunkPos, IChunk>();

        public HashChunkSource()
        {
        }

        public bool Exist(int x, int y, int z)
        {
            return Exist(new ChunkPos(x, y, z));
        }

        public bool Exist(ChunkPos pos)
        {
            return chunks.ContainsKey(pos);
        }

        public bool Get(int x, int y, int z, [NotNullWhen(true)] out IChunk? chunk)
        {
            return Get(new ChunkPos(x, y, z), out chunk);
        }

        public bool Get(ChunkPos pos, [NotNullWhen(true)] out IChunk? chunk)
        {
            return chunks.TryGetValue(pos, out chunk);
        }

        public bool Remove(int x, int y, int z)
        {
            return Remove(new ChunkPos(x, y, z));
        }

        public bool Remove(ChunkPos pos)
        {
            return chunks.Remove(pos);
        }

        public void Set(int x, int y, int z, IChunk chunk)
        {
            Set(new ChunkPos(x, y, z), chunk);
        }

        public void Set(ChunkPos pos, IChunk chunk)
        {
            chunks[pos] = chunk;
        }
    }
}
