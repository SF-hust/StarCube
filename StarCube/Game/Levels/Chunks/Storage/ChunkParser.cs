using System.Diagnostics.CodeAnalysis;

using LiteDB;

using StarCube.Utility;
using StarCube.Utility.Math;
using StarCube.Utility.Container;
using StarCube.Game.Blocks;

namespace StarCube.Game.Levels.Chunks.Storage
{
    public abstract class ChunkParser
    {
        public abstract bool TryParse(BsonDocument bson, ChunkPos pos, [NotNullWhen(true)] out Chunk? chunk);
    }

    public class DefaultChunkParser : ChunkParser
    {
        public override bool TryParse(BsonDocument bson, ChunkPos pos, [NotNullWhen(true)] out Chunk? chunk)
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

        public DefaultChunkParser()
        {
            this.chunkFactory = new PalettedChunkFactory(BlockState.GlobalBlockStateIDMap);
        }

        private readonly IChunkFactory chunkFactory;
    }
}
