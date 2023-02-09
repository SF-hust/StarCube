namespace StarCube.Data.Loading
{
    public interface IDataLoader
    {
        /// <summary>
        /// 执行 DataLoader
        /// </summary>
        /// <param name="reader"></param>
        void Run();
    }
}
