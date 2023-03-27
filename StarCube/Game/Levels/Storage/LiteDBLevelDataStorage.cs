using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using StarCube.Utility.Math;
using StarCube.Game.Levels.Chunks;

namespace StarCube.Game.Levels.Storage
{
    public sealed class LiteDBLevelDataStorage : ILevelDataStorage
    {
        public bool Contains(ChunkPos pos)
        {
            throw new NotImplementedException();
        }

        public Task<Chunk?> ReadChunk(ChunkPos pos)
        {
            throw new NotImplementedException();
        }

        public bool ReadChunkSync(ChunkPos pos, [NotNullWhen(true)] out Chunk? chunk)
        {
            throw new NotImplementedException();
        }

        public void WriteChunk(Chunk chunk)
        {
            throw new NotImplementedException();
        }
    }
}
