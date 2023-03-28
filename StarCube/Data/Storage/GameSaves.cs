using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using LiteDB;

namespace StarCube.Data.Storage
{
    public sealed class GameSaves
    {
        public LiteDatabase GetOrCreateDB(string path)
        {
            string fullPath = GetFullPath(path);
            return OpenOrCreate(path, fullPath);
        }

        public bool TryGetDB(string path, [NotNullWhen(true)] out LiteDatabase? database)
        {
            if(pathToDataBase.TryGetValue(path, out database))
            {
                return true;
            }

            string fullPath = GetFullPath(path);
            if (File.Exists(fullPath))
            {
                database = OpenOrCreate(path, fullPath);
                return true;
            }

            database = null;
            return false;
        }

        public bool TryCreateDB(string path, [NotNullWhen(true)] out LiteDatabase? database)
        {
            database = null;

            if(pathToDataBase.ContainsKey(path))
            {
                return false;
            }

            string fullPath = GetFullPath(path);
            if (File.Exists(fullPath))
            {
                return false;
            }

            database = OpenOrCreate(path, fullPath);
            pathToDataBase.Add(path, database);
            return true;
        }

        private string GetFullPath(string path)
        {
            if (relativeToFullPath.TryGetValue(path, out string fullPath))
            {
                return fullPath;
            }

            fullPath = Path.Combine(directoryPath, path.Replace('/', Path.PathSeparator));
            return fullPath;
        }

        private LiteDatabase OpenOrCreate(string relativePath, string fullPath)
        {
            ConnectionString connectionString = new ConnectionString()
            {
                Filename = fullPath,
                Collation = Collation.Binary,
                
            };
            LiteDatabase database = new LiteDatabase(connectionString);
            pathToDataBase.Add(relativePath, database);
            return database;
        }

        public GameSaves(string name, string directoryPath)
        {
            this.name = name;
            this.directoryPath = directoryPath;
            relativeToFullPath = new Dictionary<string, string>();
            pathToDataBase = new Dictionary<string, LiteDatabase>();
        }

        public readonly string name;

        public readonly string directoryPath;

        private readonly Dictionary<string, string> relativeToFullPath;

        private readonly Dictionary<string, LiteDatabase> pathToDataBase;
    }
}
