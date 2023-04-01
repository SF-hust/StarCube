using System.IO;

namespace StarCube.Data.Source
{
    public interface IDataSource
    {
        public FileInfo SourceFileInfo { get; }
    }
}
