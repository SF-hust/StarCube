using System.IO;
using System.IO.Compression;

using StarCube.Utility;
using StarCube.Data.Provider;
using StarCube.Data.Provider.DataSource;


namespace StarCube.Data
{
    public static class ZipHelper
    {
        public static bool TryGetDataEntryFromZipFile(ZipArchive zip, IDataSource source, StringID dataRegistry, string prefix, StringID id, out RawDataEntry dataEntry)
        {
            string entryName = Path.Combine(id.namspace, dataRegistry.path, prefix, id.path);
            ZipArchiveEntry zipEntry = zip.GetEntry(entryName);
            Stream stream = zipEntry.Open();
            dataEntry = new RawDataEntry(id, stream, source);
            return true;
        }
    }
}
