using System.IO;

using StarCube.Utility;
using StarCube.Data.Provider.DataSource;

namespace StarCube.Data.Provider
{
    public readonly struct RawDataEntry
    {
        /// <summary>
        /// 数据的 id
        /// </summary>
        public readonly StringID id;

        /// <summary>
        /// 数据的流
        /// </summary>
        public readonly Stream stream;

        /// <summary>
        /// 数据的来源
        /// </summary>
        public readonly IDataSource source;

        public RawDataEntry(StringID id, Stream stream, IDataSource source)
        {
            this.id = id;
            this.stream = stream;
            this.source = source;
        }
    }
}
