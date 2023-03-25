namespace StarCube.Game.Level.Loading
{
    public interface IChunkBasedLevel
    {
        public void AddAnchor(LevelLoadAnchor anchor);

        public bool RemoveAnchor(LevelLoadAnchor anchor);
    }
}
