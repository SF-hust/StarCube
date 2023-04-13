using StarCube.Utility.Math;
using StarCube.Utility.Container;
using StarCube.Game.Blocks;

namespace StarCube.Game.Levels.Chunks
{
    public sealed class PalettedChunkFactory : IChunkFactory
    {
        public Chunk CreateEmpty(ChunkPos pos)
        {
            return new PalettedChunk(pos, globalBlockStateIDMap, pool);
        }

        public Chunk Create(ChunkPos pos, int[] blockStates)
        {
            return new PalettedChunk(pos, globalBlockStateIDMap, pool, blockStates);
        }

        public Chunk Create(ChunkPos pos, BlockState[] blockStates)
        {
            return new PalettedChunk(pos, globalBlockStateIDMap, pool, blockStates);
        }

        public PalettedChunkFactory(IIDMap<BlockState> globalBlockStateIDMap)
        {
            this.globalBlockStateIDMap = globalBlockStateIDMap;
            pool = new PalettedChunkDataPool(16, 16, 4096, 4096, 1024, 256, 4096);
        }

        private readonly IIDMap<BlockState> globalBlockStateIDMap;

        private readonly PalettedChunkDataPool pool;
    }
}
