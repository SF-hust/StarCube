using StarCube.Utility;
using StarCube.Bootstrap.Attributes;
using StarCube.Core.Component;
using StarCube.Game.Blocks;
using StarCube.Game.Items;

namespace StarCube.Core.Registry
{
    /// <summary>
    /// RootRegistry 与所有内置 Registry
    /// </summary>
    /// 游戏注册流程：
    /// 1. bootstrap 期间：调用 Registries.Init()，构造 RootRegistry 与其他内置 Registry 实例，将内置 Registry 加入到 Root Registry 中
    /// 2. mod 读取完成后：获取所有 mod 自定义 Registry，将其加入到 RootRegistry 中
    /// 3. 为所有 Registry 发射注册事件，构造每个 RegistryEntry 实例并加入到 Registry 中
    /// 4. 对每个 RegistryEntry 执行构建，填充其字段，使其功能完整
    [BootstrapClass]
    public static class Registries
    {
        public static readonly RootRegistry Root = new RootRegistry();

        public static readonly Registry<Block> BLOCK = Registry<Block>.Create(Constants.DEFAULT_NAMESPACE, Constants.BLOCK_STRING);

        public static readonly Registry<Item> ITEM = Registry<Item>.Create(Constants.DEFAULT_NAMESPACE, Constants.ITEM_STRING);

        static Registries()
        {
            Root.Register(BLOCK);
            Root.Register(ITEM);
        }
    }
}
