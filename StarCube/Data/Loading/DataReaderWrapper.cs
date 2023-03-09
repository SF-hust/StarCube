using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace StarCube.Data.Loading
{
    /// <summary>
    /// 从文件流中读取数据并转换为原始类型数据格式
    /// </summary>
    /// <typeparam name="R"></typeparam>
    /// <param name="stream"></param>
    /// <param name="length"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public delegate bool RawDataReader<R>(Stream stream, [NotNullWhen(true)] out R? data)
        where R : class;

    /// <summary>
    /// 将原始类型数据转换为所需要的数据
    /// </summary>
    /// <typeparam name="D"></typeparam>
    /// <typeparam name="R"></typeparam>
    /// <param name="rawData"></param>
    /// <param name="id"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public delegate bool DataParser<D, R>(R rawData, StringID id, [NotNullWhen(true)] out D? data)
        where D : class
        where R : class;


    /// <summary>
    /// 先通过 RawDataReader 从文件流中读取原始类型数据(如 json)，再通过 DataParser 将其解析成真正需要的数据
    /// </summary>
    /// <typeparam name="D"></typeparam>
    /// <typeparam name="R"></typeparam>
    public class DataReaderWrapper<D, R> : IDataReader<D>
        where D : class
        where R : class
    {
        public bool TryReadDataFrom(Stream Stream, StringID id, [NotNullWhen(true)] out D? data)
        {
            data = null;
            return TryReadFromStream(Stream, out R? rawData) && TryParse(rawData, id, out data);
        }

        public DataReaderWrapper(RawDataReader<R> rawDataReader, DataParser<D, R> dataParser)
        {
            TryReadFromStream = rawDataReader;
            TryParse = dataParser;
        }

        private readonly RawDataReader<R> TryReadFromStream;

        private readonly DataParser<D, R> TryParse;
    }
}
