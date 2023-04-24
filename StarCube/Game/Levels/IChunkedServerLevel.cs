using StarCube.Utility.Math;
using StarCube.Game.Levels.Chunks.Loading;

namespace StarCube.Game.Levels
{
    public interface IChunkedServerLevel
    {
        /// <summary>
        /// 添加一个动态的加载锚点
        /// </summary>
        /// <param name="anchor"></param>
        public bool TryAddDynamicAnchor(ChunkLoadAnchor anchor);

        /// <summary>
        /// 移除指定的加载锚点
        /// </summary>
        /// <param name="anchor"></param>
        public bool TryRemoveDynamicAnchor(ChunkLoadAnchor anchor);

        /// <summary>
        /// 添加一个静态加载锚点
        /// </summary>
        /// <param name="anchorData"></param>
        /// <returns>新添加的锚点的 id，每次添加递增</returns>
        public long AddStaticAnchor(AnchorData anchorData);

        /// <summary>
        /// 移除指定 id 对应的加载锚点
        /// </summary>
        /// <param name="id"></param>
        /// <returns>是否存在指定 id 的锚点</returns>
        public bool TryRemoveStaticAnchor(long id);

        /// <summary>
        /// 移除指定位置的所有加载锚点
        /// </summary>
        /// <param name="pos"></param>
        /// <returns>移除的锚点数量</returns>
        public int RemoveStaticAnchorsAt(ChunkPos pos);
    }
}
