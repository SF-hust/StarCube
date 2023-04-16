using System.Diagnostics.CodeAnalysis;

using LiteDB;

using StarCube.Utility.Math;

namespace StarCube.Game.Levels.Chunks.Storage
{
    public interface IChunkParser
    {
        public bool TryParse(BsonDocument bson, ChunkPos pos, [NotNullWhen(true)] out Chunk? chunk);

        public BsonDocument ToBson(Chunk chunk);
    }
}
