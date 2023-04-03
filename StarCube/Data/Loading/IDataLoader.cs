using StarCube.Data.Provider;

namespace StarCube.Data.Loading
{
    public interface IDataLoader
    {
        /// <summary>
        /// 执行 DataLoader
        /// </summary>
        void Run(IDataProvider dataProvider);
    }
}
