namespace StarCube.Game.Level.Loading
{
    public interface IChunkBasedLevel
    {
        public void AddAnchor(ChunkLoadAnchor anchor);

        public bool RemoveAnchor(ChunkLoadAnchor anchor);
    }
}
