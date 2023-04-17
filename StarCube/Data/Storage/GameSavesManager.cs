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

        public bool TryLoadGameSaves(string name, [NotNullWhen(true)] out GameSaves? saves)
        {
            saves = null;
            string directoryName = ToValidDirectoryName(name);
            if (directoryName.Length == 0)
            {
                return false;
            }
            string fullPath = Path.Combine(rootSavePath, directoryName);
            return GameSaves.TryLoadFromDirectory(fullPath, out saves);
        }

        public bool TryCreateGameSaves(string name, [NotNullWhen(true)] out GameSaves? saves)
        {
            saves = null;
            string directoryName = ToValidDirectoryName(name);
            if (directoryName.Length == 0)
            {
                return false;
            }
            string fullPath = Path.Combine(rootSavePath, directoryName);
            if (File.Exists(fullPath))
            {
                return false;
            }
            if (GameSaves.TryGetNameFromDirectory(fullPath, out _))
            {
                return false;
            }
            Directory.CreateDirectory(fullPath);
            saves = GameSaves.CreateInDirectory(name, fullPath);
            return true;
        }

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
