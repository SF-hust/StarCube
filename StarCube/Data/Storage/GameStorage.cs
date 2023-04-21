using LiteDB;

namespace StarCube.Data.Storage
{
    /// <summary>
    /// 表示一个单一数据库文件的存储
    /// </summary>
    public abstract class GameStorage
    {
        public GameStorage(string path, LiteDatabase database)
        {
            this.path = path;
            this.database = database;
        }

        public readonly string path;

        public readonly LiteDatabase database;
    }
}
