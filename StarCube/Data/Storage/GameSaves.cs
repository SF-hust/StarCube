﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using LiteDB;

using StarCube.Utility;
using StarCube.Utility.Logging;

namespace StarCube.Data.Storage
{
    public sealed class GameSaves : IDisposable
    {
        public const string LiteDatabaseExtension = ".litedb";

        public const string MetaDataBaseName = "meta";

        public const string MetaDataCollectionName = "metadata";

        /// <summary>
        /// 在指定文件夹下创建一个存档
        /// </summary>
        /// <param name="name"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static GameSaves CreateInDirectory(string name, string path)
        {
            GameSaves saves = new GameSaves(name, path);
            LiteDatabase meta = saves.GetOrCreateDB(MetaDataBaseName);
            ILiteCollection<BsonDocument> metadataCollection = meta.GetCollection(MetaDataCollectionName, BsonAutoId.ObjectId);
            if (metadataCollection.Count() > 0)
            {
                metadataCollection.DeleteAll();
            }
            BsonDocument metadataDoc = new BsonDocument();
            metadataDoc["name"] = name;
            metadataCollection.Upsert(metadataDoc);
            return saves;
        }

        public static bool TryGetNameFromDirectory(string path, out string name)
        {
            name = string.Empty;
            string fullDBPath = Path.Combine(path, MetaDataBaseName + LiteDatabaseExtension);
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
            if (!metaDatabase.CollectionExists(MetaDataCollectionName))
            {
                return false;
            }
            ILiteCollection<BsonDocument> metadataCollection = metaDatabase.GetCollection(MetaDataCollectionName, BsonAutoId.ObjectId);
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
        /// <param name="path"></param>
        /// <param name="saves"></param>
        /// <returns></returns>
        public static bool TryLoadFromDirectory(string path, [NotNullWhen(true)] out GameSaves? saves)
        {
            saves = null;
            string fullDBPath = Path.Combine(path, MetaDataBaseName + LiteDatabaseExtension);
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
            if (!metaDatabase.CollectionExists(MetaDataCollectionName))
            {
                return false;
            }
            ILiteCollection<BsonDocument> metadataCollection = metaDatabase.GetCollection(MetaDataCollectionName, BsonAutoId.ObjectId);
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
        /// 获取或创建指定路径的 Database
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public LiteDatabase GetOrCreateDB(string path)
        {
            lock(this)
            {
                string fullPath = GetFullPath(path);
                return OpenOrCreate(path, fullPath);
            }
        }

        /// <summary>
        /// 关闭指定路径的 Database 实例
        /// </summary>
        /// <param name="path"></param>
        public void ReleaseDB(string path)
        {
            lock(this)
            {
                if (!pathToDataBase.Remove(path, out LiteDatabase? db))
                {
                    LogUtil.Error($"in game saves (\"{name}\"), tries to close database (\"{path}\") which does not exist");
                    return;
                }

                db.Dispose();
            }
        }

        /// <summary>
        /// 销毁指定路径的 Database
        /// </summary>
        /// <param name="path"></param>
        public void DropDB(string path)
        {
            lock(this)
            {
                // 释放 db 的实例
                if(pathToDataBase.Remove(path, out LiteDatabase? db))
                {
                    db.Dispose();
                }

                string fullPath = GetFullPath(path);
                relativeToFullPath.Remove(path);
                // 删除 db 文件
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
        }

        public bool TryGetDB(string path, [NotNullWhen(true)] out LiteDatabase? database)
        {
            lock(this)
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
        }

        public bool TryCreateDB(string path, [NotNullWhen(true)] out LiteDatabase? database)
        {
            lock(this)
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
        }

        private string GetFullPath(string relativePath)
        {
            if (!StringID.IsValidName(relativePath))
            {
                throw new ArgumentException("relativePath");
            }

            if (relativeToFullPath.TryGetValue(relativePath, out string fullPath))
            {
                return fullPath;
            }

            fullPath = Path.Combine(directoryPath, relativePath.Replace(StringID.PATH_SEPARATOR_CHAR, Path.PathSeparator)) + LiteDatabaseExtension;
            relativeToFullPath.Add(relativePath, fullPath);
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

        public void Dispose()
        {
            foreach (LiteDatabase db in pathToDataBase.Values)
            {
                db.Dispose();
            }
            relativeToFullPath.Clear();
            pathToDataBase.Clear();
        }

        private GameSaves(string name, string directoryPath)
        {
            this.name = name;
            this.directoryPath = directoryPath;
        }

        public readonly string name;

        public readonly string directoryPath;

        private readonly Dictionary<string, string> relativeToFullPath = new Dictionary<string, string>();

        private readonly Dictionary<string, LiteDatabase> pathToDataBase = new Dictionary<string, LiteDatabase>();
    }
}
