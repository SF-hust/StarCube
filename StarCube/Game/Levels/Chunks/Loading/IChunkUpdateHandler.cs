using StarCube.Utility.Math;

namespace StarCube.Game.Levels.Chunks.Loading
{
    public interface IChunkUpdateHandler
    {
        /// <summary>
        /// 加载一个 chunk 到内存后调用此方法
        /// </summary>
        /// <param name="chunk"></param>
        /// <param name="active">加载的 chunk 是否已经是活跃的</param>
        public void OnChunkLoad(Chunk chunk, bool active);

        /// <summary>
        /// 一个已加载 chunk 由非活跃状态转为活跃状态后调用此方法
        /// </summary>
        /// <param name="pos"></param>
        public void OnChunkActive(ChunkPos pos);

        /// <summary>
        /// 一个已加载 chunk 内容被修改后调用此方法
        /// </summary>
        /// <param name="chunk"></param>
        public void OnChunkModify(Chunk chunk);

        /// <summary>
        /// 一个 chunk 从内存中卸载后调用此方法
        /// </summary>
        /// <param name="pos"></param>
        public void OnChunkUnload(ChunkPos pos);
    }
}
