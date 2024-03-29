﻿using System.Diagnostics.CodeAnalysis;
using System.IO;

using StarCube.Utility;

namespace StarCube.Data.Loading
{
    public interface IDataReader<T>
        where T : class
    {
        /// <summary>
        /// 从文件流中读取数据并解析成特定格式
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="length"></param>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool TryReadDataFrom(Stream stream, long length, StringID id, [NotNullWhen(true)] out T? data);
    }
}
