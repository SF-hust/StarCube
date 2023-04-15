using StarCube.Utility.Math;

namespace StarCube.Game.Levels.Chunks.Loading
{
    public sealed class DummyChunkUpdateHandler : IChunkUpdateHandler
    {
        public static readonly DummyChunkUpdateHandler Instance = new DummyChunkUpdateHandler();

        public void OnChunkActive(ChunkPos pos)
        {
        }

        public void OnChunkLoad(Chunk chunk, bool active)
        {
        }

        public void OnChunkModify(Chunk chunk)
        {
        }

        public void OnChunkUnload(ChunkPos pos)
        {
        }
    }
}
