using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StarCube.Data.Provider.DataSource
{
    public class FileDataSource : IDataSource
    {
        public FileInfo SourceFileInfo => fileInfo;

        public readonly string path;

        private readonly FileInfo fileInfo;

        public FileDataSource(string path)
        {
            this.path = path;
            fileInfo = new FileInfo(path);
        }
    }
}
