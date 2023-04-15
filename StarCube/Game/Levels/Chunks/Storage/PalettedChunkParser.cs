using System.Diagnostics.CodeAnalysis;

using LiteDB;

using StarCube.Utility;
using StarCube.Utility.Math;

namespace StarCube.Game.Levels.Chunks.Storage
{
    public class PalettedChunkParser : IChunkParser
    {
        public bool TryParse(BsonDocument bson, ChunkPos pos, [NotNullWhen(true)] out Chunk? chunk)
        {
            chunk = null;

            if (!bson.TryGetInt32("blocksize", out int blockBitSize))
            {
                return false;
            }

            if (blockBitSize == 0)
            {
                chunk = chunkFactory.CreateEmpty(pos);
                return true;
            }

            if (!bson.TryGetBinary("blockstates", out byte[] binary))
            {
                return false;
            }

            if (!ChunkStorage.TryDecodeBlockStates(binary, blockBitSize, out int[] blockStates))
            {
                return false;
            }

            chunk = chunkFactory.Create(pos, blockStates);

            return true;
        }

        public BsonDocument ToBson(Chunk chunk)
        {
            BsonDocument bson = new BsonDocument();



            return bson;
        }

        public PalettedChunkParser(IChunkFactory chunkFactory)
        {
            this.chunkFactory = chunkFactory;
        }

        private readonly IChunkFactory chunkFactory;
    }
}
