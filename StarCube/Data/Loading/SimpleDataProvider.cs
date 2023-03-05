using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using static StarCube.Data.Loading.IDataProvider;

namespace StarCube.Data.Loading
{
    public class SimpleDataProvider : IDataProvider
    {
        // ~ IDataProvider 接口实现 start
        public IEnumerable<DataEntry> EnumerateData(StringID dataRegistry, DataFilterMode filterMode)
        {
            List<DataEntry> dataEntries = new List<DataEntry>();
            // 遍历所有以 modid 命名的文件夹
            foreach(string directoryForModid in Directory.EnumerateDirectories(dataDirectoryPath))
            {
                string modid = Path.GetFileName(directoryForModid);
                if(!StringID.IsValidNamespace(modid))
                {
                    continue;
                }

                string directoryPath = Path.Combine(directoryForModid, dataRegistry.path);
                GetAllDataEntriesInDirectory(directoryPath, modid, filterMode, dataEntries);
            }

            return dataEntries;
        }

        /// <summary>
        /// 递归查找文件夹及其子文件夹里的数据文件
        /// </summary>
        /// <param name="directoryPath">数据文件夹的根路径</param>
        /// <param name="modid">模组 id</param>
        /// <param name="filterMode">过滤设置</param>
        /// <param name="dataEntries">输出的数据项目</param>
        private void GetAllDataEntriesInDirectory(string directoryPath, string modid, DataFilterMode filterMode, List<DataEntry> dataEntries)
        {
            // 合法的文件名到其路径的映射
            Dictionary<string, string> fileToPath = new Dictionary<string, string>();
            // 因文件名冲突而被排除的文件
            HashSet<string> conflictFilenames = new HashSet<string>();

            // 查找本文件夹下的所有数据文件
            string searchPattern = filterMode.directoryPrefix.Replace(StringID.PATH_SEPARATOR_CHAR, Path.DirectorySeparatorChar) + "*";
            foreach (string filePath in Directory.EnumerateFiles(directoryPath, searchPattern, SearchOption.AllDirectories))
            {
                // 确认文件名合法
                string filename = Path.GetRelativePath(directoryPath, filePath).Replace(Path.DirectorySeparatorChar, StringID.PATH_SEPARATOR_CHAR);
                int dotIndex = filename.IndexOf('.');
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
                dataEntries.Add(new DataEntry(id, stream));
            }
        }

        public bool TryGet(StringID dataRegistry, StringID id, [NotNullWhen(true)] out FileStream? stream)
        {
            stream = null;

            string filePathWithoutExtension = Path.Combine(dataDirectoryPath, id.namspace, dataRegistry.path, id.path).Replace(StringID.PATH_SEPARATOR_CHAR, Path.DirectorySeparatorChar);
            string directoryPath = Path.GetDirectoryName(filePathWithoutExtension);

            bool found = false;
            string foundFilePath = string.Empty;
            foreach (string filePath in Directory.EnumerateFiles(directoryPath, filePathWithoutExtension + "*"))
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

            stream = new FileStream(foundFilePath, FileMode.Open);

            return true;
        }
        // ~ IDataProvider 接口实现 end

        public SimpleDataProvider(string dataDirectoryPath)
        {
            this.dataDirectoryPath = dataDirectoryPath;
        }

        public readonly string dataDirectoryPath;
    }
}
