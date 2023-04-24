using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using LiteDB;

using StarCube.Utility;
using StarCube.Utility.Logging;
using StarCube.Game;

namespace StarCube.Data.Storage
{
    /// <summary>
    /// 表示一个游戏存档，其中的公开成员方法均对自身加锁
    /// </summary>
    public sealed class GameSaves : IDisposable
    {
        public const string LiteDatabaseExtension = ".litedb";

        /// <summary>
        /// 在指定文件夹下创建一个存档
        /// </summary>
        /// <param name="name"> 存档的显示名称 </param>
        /// <param name="path"> 绝对文件夹路径，必须存在，且里面不可以有任何文件 </param>
        /// <returns> 创建完成的存档 </returns>
        public static GameSaves CreateInDirectory(string name, string path)
        {
            GameSaves saves = new GameSaves(name, path);
            StorageDatabase metaDatabase = saves.OpenOrCreateDatabase(ServerGameStorage.GameMetaDatabasePath);
            ILiteCollection<BsonDocument> metadataCollection = metaDatabase.Value.GetCollection(ServerGameStorage.GameMetaCollectionName, BsonAutoId.Int32);
            metadataCollection.DeleteAll();
            BsonDocument metadataDoc = new BsonDocument();
            metadataDoc["name"] = name;
            metadataCollection.Upsert(metadataDoc);
            return saves;
        }

        /// <summary>
        /// 尝试从指定目录下读取存档的名字
        /// </summary>
        /// <param name="path"> 目录路径，可以不存在 </param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool TryGetNameFromDirectory(string path, out string name)
        {
            name = string.Empty;
            string fullDBPath = Path.Combine(path, ServerGameStorage.GameMetaDatabasePath + LiteDatabaseExtension);
            if (!File.Exists(fullDBPath))
            {
                return false;
            }
            ConnectionString connectionString = new ConnectionString()
            {
                Filename = fullDBPath,
                Collation = Collation.Binary,
            };

            using LiteDatabase metaDatabase = new LiteDatabase(connectionString);
            if (!metaDatabase.CollectionExists(ServerGameStorage.GameMetaCollectionName))
            {
                return false;
            }
            ILiteCollection<BsonDocument> metadataCollection = metaDatabase.GetCollection(ServerGameStorage.GameMetaCollectionName, BsonAutoId.ObjectId);
            if (metadataCollection.Count() != 1)
            {
                return false;
            }
            BsonDocument metadataDoc = metadataCollection.FindOne(Query.All());
            return metadataDoc.TryGetString("name", out name);
        }

        /// <summary>
        /// 尝试从指定文件夹下加载一个存档
        /// </summary>
        /// <param name="path"> 存档文件夹的完整路径，不必存在 </param>
        /// <param name="saves"></param>
        /// <returns></returns>
        public static bool TryLoadFromDirectory(string path, [NotNullWhen(true)] out GameSaves? saves)
        {
            saves = null;
            string fullDBPath = Path.Combine(path, ServerGameStorage.GameMetaDatabasePath + LiteDatabaseExtension);
            if (!File.Exists(fullDBPath))
            {
                return false;
            }
            ConnectionString connectionString = new ConnectionString()
            {
                Filename = fullDBPath,
                Collation = Collation.Binary,
            };

            using LiteDatabase metaDatabase = new LiteDatabase(connectionString);
            if (!metaDatabase.CollectionExists(ServerGameStorage.GameMetaCollectionName))
            {
                return false;
            }
            ILiteCollection<BsonDocument> metadataCollection = metaDatabase.GetCollection(ServerGameStorage.GameMetaCollectionName, BsonAutoId.ObjectId);
            if (metadataCollection.Count() != 1)
            {
                return false;
            }
            BsonDocument metadataDoc = metadataCollection.FindOne(Query.All());
            if (!metadataDoc.TryGetString("name", out string name))
            {
                return false;
            }

            saves = new GameSaves(name, path);
            return true;
        }

        /// <summary>
        /// 打开或创建指定路径的 database，如果创建了新的 database，那么调用 StorageDatabase.Value 时才会真正在磁盘上创建数据库文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public StorageDatabase OpenOrCreateDatabase(string path)
        {
            CheckDisposed();
            lock (this)
            {
                if (pathToOpenedDataBaseCache.TryGetValue(path, out var database))
                {
                    return database;
                }
                string fullPath = GetFullPath(path);
                return OpenOrCreate(path, fullPath);
            }
        }

        /// <summary>
        /// 关闭 database 实例
        /// </summary>
        /// <param name="path"></param>
        internal void ReleaseDatabase(StorageDatabase database)
        {
            CheckDisposed();
            bool removed;
            lock (this)
            {
                removed = pathToOpenedDataBaseCache.Remove(database.path);
            }

            if (!removed)
            {
                LogUtil.Error($"in game saves (\"{name}\"), tries to close database (\"{database.path}\") which does not exist");
                return;
            }

            if (database.Created)
            {
                database.Value.Dispose();
            }
        }

        /// <summary>
        /// 销毁指定路径的 database，此时 database 必须未加载
        /// </summary>
        /// <param name="path"></param>
        public bool DropDatabase(string path)
        {
            CheckDisposed();
            lock (this)
            {
                // 检查数据库是否已被打开
                if (pathToOpenedDataBaseCache.ContainsKey(path))
                {
                    throw new ArgumentException("try to drop an opened database");
                }

                // 找到并删除 database 文件以及路径映射的缓存
                string fullPath = GetFullPath(path, false);
                relativeToFullPathCache.Remove(path);
                if (!File.Exists(fullPath))
                {
                    return false;
                }
                File.Delete(fullPath);
                return true;
            }
        }

        /// <summary>
        /// 尝试获取一个已存在或已加载的 Database
        /// </summary>
        /// <param name="path"></param>
        /// <param name="database"></param>
        /// <returns></returns>
        public bool TryGetDatabase(string path, [NotNullWhen(true)] out StorageDatabase? database)
        {
            CheckDisposed();
            lock (this)
            {
                if(pathToOpenedDataBaseCache.TryGetValue(path, out database))
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
        }

        /// <summary>
        /// 尝试新建一个 Database
        /// </summary>
        /// <param name="path"></param>
        /// <param name="database"></param>
        /// <returns></returns>
        public bool TryCreateDatabase(string path, [NotNullWhen(true)] out StorageDatabase? database)
        {
            CheckDisposed();
            lock (this)
            {
                database = null;

                if(pathToOpenedDataBaseCache.ContainsKey(path))
                {
                    return false;
                }

                string fullPath = GetFullPath(path);
                if (File.Exists(fullPath))
                {
                    return false;
                }

                database = OpenOrCreate(path, fullPath);
                pathToOpenedDataBaseCache.Add(path, database);
                return true;
            }
        }

        private string GetFullPath(string relativePath, bool cacheResult = true)
        {
            if (relativeToFullPathCache.TryGetValue(relativePath, out string fullPath))
            {
                return fullPath;
            }

            if (!StringID.IsValidName(relativePath))
            {
                throw new ArgumentException("is not valid path", nameof(relativePath));
            }

            fullPath = Path.Combine(directoryPath, relativePath.Replace(StringID.PATH_SEPARATOR_CHAR, Path.DirectorySeparatorChar)) + LiteDatabaseExtension;
            if (cacheResult)
            {
                relativeToFullPathCache.Add(relativePath, fullPath);
            }
            return fullPath;
        }

        private StorageDatabase OpenOrCreate(string relativePath, string fullPath)
        {
            ConnectionString connectionString = new ConnectionString()
            {
                Filename = fullPath,
                Collation = Collation.Binary,
            };

            if (File.Exists(fullPath))
            {
                LiteDatabase db = new LiteDatabase(connectionString);
                return new StorageDatabase(relativePath, this, db);
            }

            StorageDatabase database = new StorageDatabase(relativePath, this, () =>
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                return new LiteDatabase(connectionString);
            });
            pathToOpenedDataBaseCache.Add(relativePath, database);
            return database;
        }

        public void Dispose()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(GameSaves), "double dispose");
            }

            // 释放所有已打开的 database
            foreach (StorageDatabase database in pathToOpenedDataBaseCache.Values)
            {
                if (database.Created)
                {
                    database.Value.Dispose();
                }
            }

            relativeToFullPathCache.Clear();
            pathToOpenedDataBaseCache.Clear();

            disposed = true;
        }

        private void CheckDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(GameSaves), "disposed");
            }
        }

        private GameSaves(string name, string directoryPath)
        {
            this.name = name;
            this.directoryPath = directoryPath;
        }

        /// <summary>
        /// 存档的显示名称
        /// </summary>
        public readonly string name;

        /// <summary>
        /// 存档的文件夹名字
        /// </summary>
        public readonly string directoryPath;

        /// <summary>
        /// 存档中数据库相对路径到绝对路径的映射缓存
        /// </summary>
        private readonly Dictionary<string, string> relativeToFullPathCache = new Dictionary<string, string>();

        /// <summary>
        /// 存档中已打开的数据库
        /// </summary>
        private readonly Dictionary<string, StorageDatabase> pathToOpenedDataBaseCache = new Dictionary<string, StorageDatabase>();

        private volatile bool disposed = false;
    }
}
