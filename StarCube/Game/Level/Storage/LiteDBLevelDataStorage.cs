using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using StarCube.Utility.Math;
using StarCube.Game.Level.Chunk;

namespace StarCube.Game.Level.Storage
{
    public sealed class LiteDBLevelDataStorage : ILevelDataStorage
    {
        public bool Contains(ChunkPos pos)
        {
            throw new NotImplementedException();
        }

        public Task<LevelChunk?> ReadChunk(ChunkPos pos)
        {
            throw new NotImplementedException();
        }

        public bool ReadChunkSync(ChunkPos pos, [NotNullWhen(true)] out LevelChunk? chunk)
        {
            throw new NotImplementedException();
        }

        public void WriteChunk(LevelChunk chunk)
        {
            throw new NotImplementedException();
        }
    }
}
