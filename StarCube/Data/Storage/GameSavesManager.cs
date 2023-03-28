using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace StarCube.Data.Storage
{
    public sealed class GameSavesManager
    {
        private static readonly char[] invalidFileNameChar = Path.GetInvalidFileNameChars();

        public bool Contains(string name)
        {
            Update();

            return nameToGameSaves.ContainsKey(name);
        }

        public bool TryCreateGameSaves(string name, [NotNullWhen(true)] out GameSaves? gameSaves)
        {
            Update();

            gameSaves = null;

            if (name.IndexOfAny(invalidFileNameChar) != -1)
            {
                return false;
            }

            if(nameToGameSaves.ContainsKey(name))
            {
                return false;
            }

            string path = Path.Combine(rootSavePath, name);
            Directory.CreateDirectory(name);
            gameSaves = new GameSaves(name, path);

            nameToGameSaves.Add(name, gameSaves);

            return true;
        }

        public void Update()
        {
            nameToGameSaves.Clear();
            foreach(string path in Directory.EnumerateDirectories(rootSavePath, "*", SearchOption.TopDirectoryOnly))
            {
                string name = Path.GetRelativePath(rootSavePath, path);
                nameToGameSaves.TryAdd(name, new GameSaves(name, rootSavePath));
            }
        }

        public GameSavesManager(string rootSavePath)
        {
            this.rootSavePath = rootSavePath;
            nameToGameSaves = new Dictionary<string, GameSaves>();
            Update();
        }

        public readonly string rootSavePath;

        private readonly Dictionary<string, GameSaves> nameToGameSaves;
    }
}
