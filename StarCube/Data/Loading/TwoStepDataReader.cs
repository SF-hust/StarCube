using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace StarCube.Data.Loading
{
    public class TwoStepDataReader<D, S> : IDataReader<D>
        where D : class
        where S : class
    {
        /// <summary>
        /// 将文件流转换为结构化的数据
        /// </summary>
        /// <param name="fileStream"></param>
        /// <param name="id"></param>
        /// <param name="structuredData"></param>
        /// <returns></returns>
        public delegate bool DataStreamReader(FileStream fileStream, StringID id, [NotNullWhen(true)] out S? structuredData);

        /// <summary>
        /// 将结构化的数据转化为最终所需的数据
        /// </summary>
        /// <param name="structuredData"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public delegate bool DataParser(S structuredData, [NotNullWhen(true)] out D? data);


        public bool TryReadDataFrom(FileStream fileStream, StringID id, [NotNullWhen(true)] out D? data)
        {
            data = null;
            return TryReadFromStream(fileStream, id, out S? structuredData) && TryParse(structuredData, out data);
        }

        public TwoStepDataReader(TwoStepDataReader<D, S>.DataStreamReader tryReadFromStream, TwoStepDataReader<D, S>.DataParser tryParse)
        {
            TryReadFromStream = tryReadFromStream;
            TryParse = tryParse;
        }

        private readonly DataStreamReader TryReadFromStream;

        private readonly DataParser TryParse;
    }
}
