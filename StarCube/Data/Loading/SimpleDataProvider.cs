using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using StarCube.Utility;

#nullable enable

namespace StarCube.Data.Loading
{
    public class SimpleDataProvider : IDataProvider
    {
        // ~ IDataProvider 接口实现 start
        public IEnumerable<IDataProvider.DataEntry> EnumerateData(StringID dataRegistry)
        {
            List<IDataProvider.DataEntry> dataEntries = new List<IDataProvider.DataEntry>();
            foreach(string directoryForModid in Directory.EnumerateDirectories(dataDirectoryPath))
            {
                string modid = Path.GetFileName(directoryForModid);
                if(!StringID.IsValidNamespace(modid))
                {
                    continue;
                }

                string directoryPath = Path.Combine(directoryForModid, dataRegistry.path);
                GetAllDataEntriesInDirectory(directoryPath, modid, dataEntries);
            }

            return dataEntries;
        }

        /// <summary>
        /// 递归查找文件夹及其子文件夹里的数据文件
        /// </summary>
        /// <param name="directoryPath">文件夹的路径</param>
        /// <param name="prefix">应被加到</param>
        /// <param name="modid"></param>
        /// <param name="dataEntries"></param>
        private void GetAllDataEntriesInDirectory(string directoryPath, string modid, List<IDataProvider.DataEntry> dataEntries)
        {
            // 合法的文件名到其路径的映射
            Dictionary<string, string> fileToPath = new Dictionary<string, string>();
            // 因文件名冲突而被排除的文件
            HashSet<string> conflictFilenames = new HashSet<string>();

            // 查找本文件夹下的所有数据文件
            foreach (string filePath in Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories))
            {
                // 确认文件名合法
                string filename = Path.GetRelativePath(directoryPath, filePath).Replace('\\', '/');
                int dotIndex = filename.SimpleIndexOf('.');
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
                Stream stream = new FileStream(filePath, FileMode.Open);
                dataEntries.Add(new IDataProvider.DataEntry(id, stream));
            }
        }

        public void Refresh()
        {
        }

        public bool TryGet(StringID dataRegistry, StringID id, out IDataProvider.DataEntry entry)
        {
            entry = new IDataProvider.DataEntry();

            string filePathWithoutExtension = Path.Combine(dataDirectoryPath, id.namspace, dataRegistry.path, id.path).Replace('/', '\\');
            string directoryPath = Path.GetDirectoryName(filePathWithoutExtension);

            bool found = false;
            string foundFilePath = string.Empty;
            foreach (string filePath in Directory.EnumerateFiles(directoryPath))
            {
                if (filePath.StartsWith(filePathWithoutExtension, StringComparison.Ordinal) &&
                    filePath.SimpleIndexOf('.') == filePathWithoutExtension.Length)
                {
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

            entry = new IDataProvider.DataEntry(id, new FileStream(foundFilePath, FileMode.Open));

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
