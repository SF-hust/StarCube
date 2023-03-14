using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using StarCube.Utility.Math;

namespace StarCube.Game.Level.Chunk
{
    public class ChunkMap
    {
        public int Count => posToChunk.Count;

        public bool Exist(ChunkPos pos)
        {
            return posToChunk.ContainsKey(pos);
        }

        public bool TryGet(ChunkPos pos, [NotNullWhen(true)] out IChunk? chunk)
        {
            return posToChunk.TryGetValue(pos, out chunk);
        }

        public void Put(ChunkPos pos, IChunk chunk)
        {
            posToChunk.Add(pos, chunk);
        }

        public bool Remove(ChunkPos pos)
        {
            return posToChunk.Remove(pos);
        }

        public bool Exist(int x, int y, int z)
        {
            return posToChunk.ContainsKey(new ChunkPos(x, y, z));
        }

        public bool TryGet(int x, int y, int z, [NotNullWhen(true)] out IChunk? chunk)
        {
            return posToChunk.TryGetValue(new ChunkPos(x, y, z), out chunk);
        }

        public void Put(int x, int y, int z, IChunk chunk)
        {
            posToChunk.Add(new ChunkPos(x, y, z), chunk);
        }

        public bool Remove(int x, int y, int z)
        {
            return posToChunk.Remove(new ChunkPos(x, y, z));
        }

        public ChunkMap()
        {
            posToChunk = new Dictionary<ChunkPos, IChunk>();
        }

        private readonly Dictionary<ChunkPos, IChunk> posToChunk;
    }
}
