using StarCube.Utility;
using StarCube.Bootstrap.Attributes;
using StarCube.Core.Components;
using StarCube.Core.Registries;
using StarCube.Game.Blocks;
using StarCube.Game.Items;
using StarCube.Game.BlockEntities;
using StarCube.Game.Entities;

[assembly: RegisterBootstrapClass(typeof(BuiltinRegistries))]
namespace StarCube.Core.Registries
{
    /// <summary>
    /// RootRegistry 与所有内置 Registry
    /// </summary>
    /// 游戏注册流程：
    /// 1. bootstrap 期间：调用 Registries.Init()，构造 RootRegistry 与其他内置 Registry 实例，将内置 Registry 加入到 Root Registry 中
    /// 2. mod 读取完成后：获取所有 mod 自定义 Registry，将其加入到 RootRegistry 中
    /// 3. 为所有 Registry 发射注册事件，构造每个 RegistryEntry 实例并加入到 Registry 中
    /// 4. 对每个 RegistryEntry 执行构建，填充其字段，使其功能完整
    public static class BuiltinRegistries
    {
        public static readonly RootRegistry Root = new RootRegistry();

        public static readonly Registry<Block> Block = Registry<Block>.Create(Constants.DEFAULT_NAMESPACE, Constants.BLOCK_STRING);

        public static readonly Registry<Item> Item = Registry<Item>.Create(Constants.DEFAULT_NAMESPACE, Constants.ITEM_STRING);

        public static readonly Registry<BlockEntityType> BlockEntityType = Registry<BlockEntityType>.Create(Constants.DEFAULT_NAMESPACE, Constants.BLOCKENTITY_TYPE_STRING);

        public static readonly Registry<EntityType> EntityType = Registry<EntityType>.Create(Constants.DEFAULT_NAMESPACE, Constants.ENTITY_TYPE_STRING);

        public static readonly Registry<ComponentType<Block>> BlockComponentType = Registry<ComponentType<Block>>.Create(Constants.DEFAULT_NAMESPACE, "block_component_type");

        public static readonly Registry<ComponentType<BlockEntity>> BlockEntityComponentType = Registry<ComponentType<BlockEntity>>.Create(Constants.DEFAULT_NAMESPACE, "blockentity_component_type");

        public static readonly Registry<ComponentType<Entity>> EntityComponentType = Registry<ComponentType<Entity>>.Create(Constants.DEFAULT_NAMESPACE, "entity_component_type");


        static BuiltinRegistries()
        {
            Root.Register(Block);
            Root.Register(Item);
            Root.Register(BlockEntityType);
            Root.Register(EntityType);
            Root.Register(BlockComponentType);
            Root.Register(BlockEntityComponentType);
            Root.Register(EntityComponentType);
        }
    }
}
