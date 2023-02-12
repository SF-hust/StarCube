using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StarCube.Core.Data.Loading
{
    public interface IDataProvider
    {
        public IEnumerable<Stream> DataStreams { get; }
    }
}
