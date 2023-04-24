using System;
using LiteDB;

namespace StarCube.Data.Storage
{
    /// <summary>
    /// 对 LiteDatabase 的封装
    /// </summary>
    public sealed class StorageDatabase
    {
        /// <summary>
        /// 数据库是否已被创建
        /// </summary>
        public bool Created => database.IsValueCreated;

        /// <summary>
        /// 获取一个已创建的 LiteDatabase 实例
        /// </summary>
        public LiteDatabase Value => database.Value;

        /// <summary>
        /// 释放数据库，如果数据库没有被创建，那释放后也不会自动创建
        /// </summary>
        public void Release()
        {
            saves.ReleaseDatabase(this);
        }

        internal StorageDatabase(string path, GameSaves saves, LiteDatabase database)
        {
            this.path = path;
            this.saves = saves;
            this.database = new Lazy<LiteDatabase>(database);
        }

        internal StorageDatabase(string path, GameSaves saves, Func<LiteDatabase> databaseFactory)
        {
            this.path = path;
            this.saves = saves;
            database = new Lazy<LiteDatabase>(databaseFactory, true);
        }

        /// <summary>
        /// 数据库文档在存档中的相对路径
        /// </summary>
        public readonly string path;

        private readonly GameSaves saves;

        private readonly Lazy<LiteDatabase> database;
    }
}
