using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace StarCube.Data.Loading
{
    public interface IDataReader<T>
        where T : class
    {
        /// <summary>
        /// 从文件流中读取数据并解析成特定格式
        /// </summary>
        /// <param name="fileStream"></param>
        /// <param name="dataRegistry"></param>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool TryReadDataFrom(Stream stream, StringID id, [NotNullWhen(true)] out T? data);
    }
}
