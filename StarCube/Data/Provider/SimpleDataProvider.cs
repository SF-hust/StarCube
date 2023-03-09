using System;
using System.Collections.Generic;
using System.IO;

using StarCube.Utility;
using StarCube.Data.Provider.DataSource;

namespace StarCube.Data.Provider
{
    public class SimpleDataProvider : IDataProvider
    {
        // ~ IDataProvider 接口实现 start
        bool IDataProvider.TryGetData(string modid, string registry, string directory, string entryName, out RawDataEntry dataEntry)
        {
            dataEntry = new RawDataEntry();

            string filePathWithoutExtension = Path.Combine(dataDirectoryPath, directory, entryName).Replace(StringID.PATH_SEPARATOR_CHAR, Path.DirectorySeparatorChar);
            string directoryPath = Path.GetDirectoryName(filePathWithoutExtension);
            string searchPattern = Path.GetFileName(entryName) + "*";

            bool found = false;
            string foundFilePath = string.Empty;
            foreach (string filePath in Directory.EnumerateFiles(directoryPath, searchPattern, SearchOption.TopDirectoryOnly))
            {
                if (filePath.StartsWith(filePathWithoutExtension, StringComparison.Ordinal) &&
                    (filePath.Length == filePathWithoutExtension.Length ||
                    filePath.IndexOf('.') == filePathWithoutExtension.Length))
                {
                    // 多个文件命名冲突，视为不存在这个文件
                    if (found)
                    {
                        return false;
                    }

                    found = true;
                    foundFilePath = filePath;
                }
            }

            if (!found)
            {
                return false;
            }

            string registryPath = Path.Combine(dataDirectoryPath, modid, registry);
            string entryPath = Path.GetRelativePath(registryPath, filePathWithoutExtension).Replace(Path.DirectorySeparatorChar, StringID.PATH_SEPARATOR_CHAR);

            if (!StringID.TryCreate(modid, entryPath, out StringID id))
            {
                return false;
            }
            Stream stream = new FileStream(foundFilePath, FileMode.Open);
            IDataSource source = new FileDataSource(foundFilePath);
            dataEntry = new RawDataEntry(id, stream, source);

            return true;
        }

        bool IDataProvider.TryGetDataChain(string modid, string registry, string directory, string entryName, List<RawDataEntry> dataEntryChain)
        {
            if((this as IDataProvider).TryGetData(modid, registry, directory, entryName, out RawDataEntry dataEntry))
            {
                dataEntryChain.Add(dataEntry);
                return true;
            }
            return false;
        }

        void IDataProvider.EnumerateData(string registry, string directory, List<RawDataEntry> dataEntries)
        {
            // 遍历所有符合 modid 格式的文件夹
            foreach (string directoryForModid in Directory.EnumerateDirectories(dataDirectoryPath))
            {
                string modid = Path.GetFileName(directoryForModid);
                if (!StringID.IsValidNamespace(modid))
                {
                    continue;
                }

                string registryPath = Path.Combine(directoryForModid, registry);
                string directoryPath = Path.Combine(directoryForModid, directory);
                GetAllDataEntriesInDirectory(modid, registryPath, directoryPath, dataEntries);
            }
        }

        void IDataProvider.EnumerateDataChain(string registry, string directory, List<List<RawDataEntry>> dataEntryChains)
        {
            throw new NotImplementedException();
        }

        // ~ IDataProvider 接口实现 end

        /// <summary>
        /// 递归查找文件夹及其子文件夹里的数据文件
        /// </summary>
        /// <param name="modid">模组 id</param>
        /// <param name="registryPath">数据 registry 目录路径，用于计算数据的 id</param>
        /// <param name="directoryPath">要遍历的目录路径</param>
        /// <param name="dataEntries">输出的数据项目</param>
        private void GetAllDataEntriesInDirectory(string modid, string registryPath, string directoryPath, List<RawDataEntry> dataEntries)
        {
            // 合法的文件名到其路径的映射
            Dictionary<string, string> fileToPath = new Dictionary<string, string>();
            // 因文件名冲突而被排除的文件
            HashSet<string> conflictFilenames = new HashSet<string>();

            // 查找本文件夹下的所有数据文件
            foreach (string filePath in Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories))
            {
                // 确认文件名合法
                string filename = Path.GetRelativePath(registryPath, filePath).Replace(Path.DirectorySeparatorChar, StringID.PATH_SEPARATOR_CHAR);
                int dotIndex = filename.IndexOf('.');
                dotIndex = dotIndex == -1 ? filename.Length : dotIndex;
                if (!StringID.IsValidPath(filename, 0, dotIndex))
                {
                    continue;
                }

                // 获取无扩展名的文件名，并检查是否是被排除的文件
                string filenameWithoutExtension = filename[0..dotIndex];
                if (conflictFilenames.Contains(filenameWithoutExtension))
                {
                    continue;
                }

                // 检查是否有冲突
                if (fileToPath.Remove(filenameWithoutExtension))
                {
                    conflictFilenames.Add(filenameWithoutExtension);
                    continue;
                }

                // 将合法的文件名字及其路径保存
                fileToPath.Add(filenameWithoutExtension, filePath);
            }

            // 将所有找到的合法文件转化为 IDataProvider.DataEntry 并记录
            foreach (KeyValuePair<string, string> pair in fileToPath)
            {
                string filenameWithoutExtension = pair.Key;
                string filePath = pair.Value;

                StringID id = StringID.Create(modid, filenameWithoutExtension);
                FileStream stream = new FileStream(filePath, FileMode.Open);
                IDataSource source = new FileDataSource(filePath);
                dataEntries.Add(new RawDataEntry(id, stream, source));
            }
        }

        public SimpleDataProvider(string dataDirectoryPath)
        {
            this.dataDirectoryPath = dataDirectoryPath;
        }

        public readonly string dataDirectoryPath;
    }
}
