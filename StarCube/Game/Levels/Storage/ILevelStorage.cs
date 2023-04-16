using System;
using System.Diagnostics.CodeAnalysis;

using StarCube.Utility.Math;
using StarCube.Game.Levels.Chunks;

namespace StarCube.Game.Levels.Storage
{
    public interface ILevelStorage : IDisposable
    {
        public bool Contains(ChunkPos pos);

        public bool TryLoadChunk(ChunkPos pos, [NotNullWhen(true)] out Chunk? chunk);

        public void WriteChunk(Chunk chunk);
    }
}
