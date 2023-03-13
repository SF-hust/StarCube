using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

using StarCube.Utility;
using StarCube.Data.DataSource;

namespace StarCube.Data.Provider
{
    public class ZipDataProvider : IDataProvider
    {
        /* ~ IDataProvider 接口实现 start ~ */
        bool IDataProvider.TryGetData(string modid, string registry, string path, out RawDataEntry dataEntry)
        {
            dataEntry = new RawDataEntry();

            // 文件相对于 directory 目录的路径去掉扩展名
            string entryPath = Path.Combine(modid, registry, path).Replace(Path.DirectorySeparatorChar, StringID.PATH_SEPARATOR_CHAR);

            if(!entryPathToZipEntry.TryGetValue(entryPath, out ZipArchiveEntry? zipEntry))
            {
                return false;
            }

            if (!StringID.TryCreate(modid, path, out StringID id))
            {
                return false;
            }

            dataEntry = new RawDataEntry(id, zipEntry.Open(), zipEntry.Length, dataSource);

            return true;
        }

        void IDataProvider.EnumerateData(string registry, string directory, List<RawDataEntry> dataEntries)
        {
            foreach (var pair in entryPathToZipEntry)
            {
                string entryPath = pair.Key;

                string[] entryPathSplits = entryPath.Split(StringID.PATH_SEPARATOR_CHAR);

                // 检查路径是否符合要求
                if(entryPathSplits.Length < 3 || !registry.Equals(entryPathSplits[1], StringComparison.Ordinal))
                {
                    continue;
                }

                string path = Path.Combine(entryPathSplits[2..]).Replace(Path.DirectorySeparatorChar, StringID.PATH_SEPARATOR_CHAR);
                if(!StringID.TryCreate(entryPathSplits[0], path, out StringID id))
                {
                    continue;
                }

                RawDataEntry dataEntry = new RawDataEntry(id, pair.Value.Open(), pair.Value.Length, dataSource);
                dataEntries.Add(dataEntry);
            }
        }
        /* ~ IDataProvider 接口实现 end ~ */


        /// <summary>
        /// 收集 zip 文档中所有合法的文件
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, ZipArchiveEntry> GatherDataEntries()
        {
            Dictionary<string, ZipArchiveEntry> entryPathToZipEntry = new Dictionary<string, ZipArchiveEntry>();
            HashSet<string> conflictEntryPaths = new HashSet<string>();

            foreach (ZipArchiveEntry zipEntry in zip.Entries)
            {
                // 文件相对于 zip 文档根目录路径去掉扩展名
                string filePath = Path.ChangeExtension(zipEntry.FullName, null).Replace(Path.DirectorySeparatorChar, StringID.PATH_SEPARATOR_CHAR);

                if(!filePath.StartsWith(directory, StringComparison.Ordinal))
                {
                    continue;
                }

                // 文件相对于 directory 路径去掉扩展名
                string entryPath = Path.GetRelativePath(directory, filePath).Replace(Path.DirectorySeparatorChar, StringID.PATH_SEPARATOR_CHAR);

                // 检查冲突
                if(conflictEntryPaths.Contains(entryPath))
                {
                    continue;
                }
                if(entryPathToZipEntry.Remove(entryPath))
                {
                    conflictEntryPaths.Add(entryPath);
                }

                // 保存
                entryPathToZipEntry.Add(entryPath, zipEntry);
            }

            return entryPathToZipEntry;
        }

        public ZipDataProvider(string zipPath, string directory)
        {
            zip = ZipFile.OpenRead(zipPath);
            this.directory = directory;
            entryPathToZipEntry = GatherDataEntries();
            dataSource = new FileDataSource(zipPath);
        }

        private readonly ZipArchive zip;

        private readonly string directory;

        private readonly Dictionary<string, ZipArchiveEntry> entryPathToZipEntry;

        private readonly IDataSource dataSource;
    }
}
