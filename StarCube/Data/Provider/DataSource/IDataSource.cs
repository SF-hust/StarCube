using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StarCube.Data.Provider.DataSource
{
    public interface IDataSource
    {
        public FileInfo SourceFileInfo { get; }
    }
}
