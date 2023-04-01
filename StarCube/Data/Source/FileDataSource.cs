using System.IO;

namespace StarCube.Data.Source
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
