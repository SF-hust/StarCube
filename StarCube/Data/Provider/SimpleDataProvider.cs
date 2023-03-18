using System;
using System.Collections.Generic;
using System.IO;

using StarCube.Utility;
using StarCube.Data.DataSource;

namespace StarCube.Data.Provider
{
    public class SimpleDataProvider : IDataProvider
    {
        /* ~ IDataProvider 接口实现 start ~ */
        bool IDataProvider.TryGetData(string modid, string registry, string path, out RawDataEntry dataEntry)
        {
            dataEntry = new RawDataEntry();

            // 数据文件绝对路径去掉扩展名
            string filePathWithoutExtension = Path.Combine(dataDirectoryPath, modid, registry, path).Replace(StringID.PATH_SEPARATOR_CHAR, Path.DirectorySeparatorChar);
            // 数据文件所在文件夹绝对路径
            string directory = Path.GetDirectoryName(filePathWithoutExtension);
            // 数据文件名去掉扩展名 + "*"
            string searchPattern = Path.GetFileName(path) + "*";

            bool found = false;
            string foundFilePath = string.Empty;

            // 要先检查目录是否存在，否则可能抛异常
            if(!Directory.Exists(directory))
            {
                return false;
            }

            foreach (string filePath in Directory.EnumerateFiles(directory, searchPattern, SearchOption.TopDirectoryOnly))
            {
                // 判断文件是否匹配
                if (filePath.StartsWith(filePathWithoutExtension, StringComparison.Ordinal) &&
                    (filePath.Length == filePathWithoutExtension.Length ||
                    filePath.IndexOf('.', StringComparison.Ordinal) == filePathWithoutExtension.Length))
                {
                    // 多个文件成功匹配，产生冲突，认为不存在这个文件
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

            if (!StringID.TryCreate(modid, path, out StringID id))
            {
                return false;
            }
            FileStream stream = new FileStream(foundFilePath, FileMode.Open);
            long length = stream.Length;
            IDataSource source = new FileDataSource(foundFilePath);
            dataEntry = new RawDataEntry(id, stream, length, source);

            return true;
        }

        void IDataProvider.EnumerateData(string registry, string directory, List<RawDataEntry> dataEntries)
        {
            if(!Directory.Exists(dataDirectoryPath))
            {
                return;
            }

            // 遍历所有符合 modid 格式的文件夹
            foreach (string modidDirectory in Directory.EnumerateDirectories(dataDirectoryPath))
            {
                // 获取并检查文件夹名字是否符合 modid 格式
                string modid = Path.GetFileName(modidDirectory);
                if (!StringID.IsValidModid(modid))
                {
                    continue;
                }

                // registry 文件夹的绝对路径
                string registryPath = Path.Combine(modidDirectory, registry);
                // 要搜索的文件夹绝对路径
                string searchDirectory = Path.Combine(modidDirectory, directory);

                GetAllDataEntriesInDirectory(modid, registryPath, searchDirectory, dataEntries);
            }
        }
        /* ~ IDataProvider 接口实现 end ~ */


        /// <summary>
        /// 递归查找文件夹及其子文件夹里的数据文件
        /// </summary>
        /// <param name="modid">模组 id</param>
        /// <param name="registryPath"> registry 目录绝对路径，用于计算数据的 id</param>
        /// <param name="searchDirectory">要遍历的目录绝对路径</param>
        /// <param name="dataEntries">输出的数据项目</param>
        private void GetAllDataEntriesInDirectory(string modid, string registryPath, string searchDirectory, List<RawDataEntry> dataEntries)
        {
            // 合法的数据文件 id.path 到其绝对文件路径的映射
            Dictionary<string, string> entryToFilePath = new Dictionary<string, string>();
            // 因文件名冲突而被排除的文件
            HashSet<string> conflictFiles = new HashSet<string>();

            if(!Directory.Exists(searchDirectory))
            {
                return;
            }

            // 遍历本文件夹下的所有文件
            foreach (string filePath in Directory.EnumerateFiles(searchDirectory, "*", SearchOption.AllDirectories))
            {
                string entryFileName = Path.GetRelativePath(registryPath, filePath).Replace(Path.DirectorySeparatorChar, StringID.PATH_SEPARATOR_CHAR);
                // 文件名去掉扩展名后的长度
                int length = entryFileName.IndexOf('.');
                length = length == -1 ? entryFileName.Length : length;
                // 检查文件名是否合法
                if (!StringID.IsValidName(entryFileName.AsSpan(0, length)))
                {
                    continue;
                }

                // 获取无扩展名的文件名，并检查是否是被排除的文件
                string entryPath = entryFileName[0..length];
                if (conflictFiles.Contains(entryPath))
                {
                    continue;
                }

                // 检查是否有冲突
                if (entryToFilePath.Remove(entryPath))
                {
                    conflictFiles.Add(entryPath);
                    continue;
                }

                // 保存合法的文件名字及其路径
                entryToFilePath.Add(entryPath, filePath);
            }

            // 将所有找到的合法文件转化为 IDataProvider.DataEntry 并记录
            foreach (KeyValuePair<string, string> pair in entryToFilePath)
            {
                string entryPath = pair.Key;
                string filePath = pair.Value;

                StringID id = StringID.Create(modid, entryPath);
                FileStream stream = new FileStream(filePath, FileMode.Open);
                long length = stream.Length;
                IDataSource source = new FileDataSource(filePath);
                dataEntries.Add(new RawDataEntry(id, stream, length, source));
            }
        }

        public SimpleDataProvider(string dataDirectoryPath)
        {
            this.dataDirectoryPath = dataDirectoryPath;
        }

        public readonly string dataDirectoryPath;
    }
}
