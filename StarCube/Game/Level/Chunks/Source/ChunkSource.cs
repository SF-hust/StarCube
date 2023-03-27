using System.Diagnostics.CodeAnalysis;

using StarCube.Utility.Math;
using StarCube.Game.Ticking;

namespace StarCube.Game.Level.Chunks.Source
{
    public abstract class ChunkSource : ITickable
    {
        /// <summary>
        /// 返回某个 chunk 是否已经被加载到内存中，可以立即获取
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public abstract bool HasChunk(ChunkPos pos);

        /// <summary>
        /// 尝试获取一个 chunk
        /// </summary>
        /// <param name="pos">chunk 的坐标</param>
        /// <param name="load">是否要同步加载 chunk</param>
        /// <param name="levelChunk">返回的 chunk</param>
        /// <returns>true 如果指定位置的 chunk 已经在内存中；否则 false</returns>
        public abstract bool TryGetChunk(ChunkPos pos, bool load, [NotNullWhen(true)] out Chunk? chunk);

        /// <summary>
        /// 每刻更新
        /// </summary>
        public abstract void Tick();
    }
}
