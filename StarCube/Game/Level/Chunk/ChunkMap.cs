using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using StarCube.Utility.Math;

namespace StarCube.Game.Level.Chunk
{
    public struct ChunkMapEntry
    {
        public LevelChunk? chunk;
        public Task<LevelChunk>? loadingChunk;
        public int loadTicket;
        public int maxTicket;
    }


    public sealed class ChunkMap
    {
        public int Count => posToChunk.Count;

        public IEnumerable<LevelChunk> Chunks => posToChunk.Values;

        public bool Contains(ChunkPos pos)
        {
            return posToChunk.ContainsKey(pos);
        }

        public bool TryGet(ChunkPos pos, [NotNullWhen(true)] out LevelChunk? chunk)
        {
            return posToChunk.TryGetValue(pos, out chunk);
        }

        public void Add(ChunkPos pos, LevelChunk chunk)
        {
            posToChunk.Add(pos, chunk);
        }

        public bool Remove(ChunkPos pos)
        {
            return posToChunk.Remove(pos);
        }

        public bool Contains(int x, int y, int z)
        {
            return posToChunk.ContainsKey(new ChunkPos(x, y, z));
        }

        public bool TryGet(int x, int y, int z, [NotNullWhen(true)] out LevelChunk? chunk)
        {
            return posToChunk.TryGetValue(new ChunkPos(x, y, z), out chunk);
        }

        public void Put(int x, int y, int z, LevelChunk chunk)
        {
            posToChunk.Add(new ChunkPos(x, y, z), chunk);
        }

        public bool Remove(int x, int y, int z)
        {
            return posToChunk.Remove(new ChunkPos(x, y, z));
        }

        public ChunkMap()
        {
            posToChunk = new Dictionary<ChunkPos, LevelChunk>();
            posToChunkEntry = new Dictionary<ChunkPos, ChunkMapEntry>();
        }

        private readonly Dictionary<ChunkPos, LevelChunk> posToChunk;
        private readonly Dictionary<ChunkPos, ChunkMapEntry> posToChunkEntry;
    }
}
