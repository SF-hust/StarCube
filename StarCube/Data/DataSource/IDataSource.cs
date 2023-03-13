using System.IO;

namespace StarCube.Data.DataSource
{
    public interface IDataSource
    {
        public FileInfo SourceFileInfo { get; }
    }
}
