using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

using StarCube.Utility;

namespace StarCube.Data.Storage
{
    public sealed class GameSavesManager
    {
        private static readonly char[] invalidFileNameChar = Path.GetInvalidFileNameChars();

        public static bool IsValidpath(string path)
        {
            return !path.AsSpan().Contains(invalidFileNameChar, StringComparison.Ordinal);
        }

        public static string ToValidDirectoryName(string name)
        {
            StringBuilder stringBuilder = StringUtil.StringBuilder;
            bool hasInvalid = false;
            foreach (char c in name)
            {
                if (invalidFileNameChar.AsSpan().IndexOf(c) != -1)
                {
                    hasInvalid = true;
                    continue;
                }

                stringBuilder.Append(c);
            }

            if (!hasInvalid)
            {
                return name;
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// 在指定的目录中加载游戏存档
        /// </summary>
        /// <param name="path"></param>
        /// <param name="saves"></param>
        /// <returns></returns>
        public bool TryLoadGameSaves(string path, [NotNullWhen(true)] out GameSaves? saves)
        {
            string fullPath = Path.Combine(rootSavePath, path);
            return GameSaves.TryLoadFromDirectory(fullPath, out saves);
        }

        /// <summary>
        /// 尝试创建一个指定名字的游戏存档
        /// </summary>
        /// <param name="name"></param>
        /// <param name="saves"></param>
        /// <returns></returns>
        public bool TryCreateGameSaves(string name, [NotNullWhen(true)] out GameSaves? saves)
        {
            saves = null;

            // 根据存档名生成合法的目录名
            string directoryName = ToValidDirectoryName(name);
            if (directoryName.Length == 0)
            {
                return false;
            }
            string fullPath = Path.Combine(rootSavePath, directoryName);

            // 如果有同名文件就删除
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            // 如果目录下已存在一个合法的存档则创建失败
            if (GameSaves.TryGetNameFromDirectory(fullPath, out _))
            {
                return false;
            }

            // 如果目录不是一个合法存档但存在，删除其中所有内容
            if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, true);
            }
            Directory.CreateDirectory(fullPath);

            // 创建存档
            saves = GameSaves.CreateInDirectory(name, fullPath);
            return true;
        }

        /// <summary>
        /// 枚举存档根目录下的所有存档
        /// </summary>
        /// <param name="allSaves"> 所有存档的 (path, name) </param>
        public void EnumerateAllGameSaves(List<KeyValuePair<string, string>> allSaves)
        {
            foreach (string path in Directory.EnumerateDirectories(rootSavePath, "*", SearchOption.TopDirectoryOnly))
            {
                if (GameSaves.TryGetNameFromDirectory(path, out var name))
                {
                    allSaves.Add(new KeyValuePair<string, string>(path, name));
                }
            }
        }

        public GameSavesManager(string rootSavePath)
        {
            this.rootSavePath = rootSavePath;
        }

        public readonly string rootSavePath;
    }
}
