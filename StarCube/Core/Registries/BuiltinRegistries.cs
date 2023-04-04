using StarCube.Utility;
using StarCube.Bootstrap.Attributes;
using StarCube.Core.Components;
using StarCube.Game.Blocks;
using StarCube.Game.Items;
using StarCube.Game.BlockEntities;
using StarCube.Game.Entities;
using StarCube.Core.Registries;

[assembly: BootstrapClass(typeof(BuiltinRegistries))]
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

        public static readonly Registry<ComponentType<Block>> BLOCK_COMPONENT_TYPE = Registry<ComponentType<Block>>.Create(Constants.DEFAULT_NAMESPACE, "component_type/block");

        public static readonly Registry<ComponentType<ItemStack>> ITEMSTACK_COMPONENT_TYPE = Registry<ComponentType<ItemStack>>.Create(Constants.DEFAULT_NAMESPACE, "component_type/item_stack");

        public static readonly Registry<ComponentType<BlockEntity>> BLOCK_ENTITY_COMPONENT_TYPE = Registry<ComponentType<BlockEntity>>.Create(Constants.DEFAULT_NAMESPACE, "component_type/block_entity");

        public static readonly Registry<ComponentType<Entity>> ENTITY_COMPONENT_TYPE = Registry<ComponentType<Entity>>.Create(Constants.DEFAULT_NAMESPACE, "component_type/entity");

        public static readonly Registry<Block> BLOCK = Registry<Block>.Create(Constants.DEFAULT_NAMESPACE, Constants.BLOCK_STRING);

        public static readonly Registry<Item> ITEM = Registry<Item>.Create(Constants.DEFAULT_NAMESPACE, Constants.ITEM_STRING);

        public static readonly Registry<BlockEntityType> BLOCK_ENTITY_TYPE = Registry<BlockEntityType>.Create(Constants.DEFAULT_NAMESPACE, Constants.BLOCK_ENTITY_TYPE_STRING);

        public static readonly Registry<EntityType> ENTITY_TYPE = Registry<EntityType>.Create(Constants.DEFAULT_NAMESPACE, Constants.ENTITY_TYPE_STRING);

        static BuiltinRegistries()
        {
            Root.Register(BLOCK);
            Root.Register(ITEM);
        }
    }
}
