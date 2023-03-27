namespace StarCube.Game.Levels.Loading
{
    public interface IChunkBasedLevel
    {
        public void AddAnchor(ChunkLoadAnchor anchor);

        public bool RemoveAnchor(ChunkLoadAnchor anchor);
    }
}
