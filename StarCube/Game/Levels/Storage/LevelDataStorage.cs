using System.Diagnostics.CodeAnalysis;

using StarCube.Utility.Math;
using StarCube.Game.Levels.Chunks;
using LiteDB;
using System;
using StarCube.Utility.Container;
using StarCube.Game.Blocks;
using System.Collections.Generic;

namespace StarCube.Game.Levels.Storage
{
    public class RegionMap
    {

    }

    public class LevelDataStorage
    {
        public bool Contains(ChunkPos pos)
        {
            throw new NotImplementedException();
        }

        public bool TryLoadChunk(ChunkPos pos, [NotNullWhen(true)] out Chunk? chunk)
        {
            throw new NotImplementedException();
        }

        public void WriteChunk(Chunk chunk)
        {
            throw new NotImplementedException();
        }
    }
}
