using System;
using System.Collections.Generic;

using StarCube.Resource;

namespace StarCube.Core.Registry
{
    /// <summary>
    /// 用于管理游戏对象注册, 会执行真正的注册动作
    /// </summary>
    /// <typeparam name="T">class of registry entry</typeparam>
    public class DeferredRegister<T>
        where T : class, IRegistryEntry<T>
    {
        /// <summary>
        /// 创建一个指定 modid 的 DeferredRegister, 后续使用 Register() 注册时会默认使用这个 modid,
        /// 如果想向其中注册一个使用其他 modid 的 Entry, 使用 RegisterCustom()
        /// </summary>
        /// <param name="modid">your modid</param>
        /// <returns></returns>
        public static DeferredRegister<T> Create(Registry<T> registry, string modid)
        {
            return new DeferredRegister<T>(modid, registry);
        }

        private readonly string modid;
        private readonly Registry<T> registry;
        private readonly List<ResourceLocation> entries = new List<ResourceLocation>();

        private DeferredRegister(string modid, Registry<T> registry)
        {
            this.modid = modid;
            this.registry = registry;
            registry.OnRegisterEvent += DoRegister;
        }

        /// <summary>
        /// 向 DeferredRegister 中添加一个新的 Entry
        /// </summary>
        /// <param name="name">name of registry entry</param>
        public void Register(string name)
        {
            ResourceLocation id = ResourceLocation.Create(modid, name);
            if (entries.Contains(id))
            {
                throw new InvalidOperationException("can't add entry with same id to DeferredRegister");
            }
            entries.Add(id);
        }

        /// <summary>
        /// 向 DeferredRegister 中添加一个新的 Entry, 但自行指定 modid
        /// </summary>
        /// <param name="modid"></param>
        /// <param name="name"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void RegisterCustom(string modid, string name)
        {
            ResourceLocation id = ResourceLocation.Create(modid, name);
            if (entries.Contains(id))
            {
                throw new InvalidOperationException("can't add entry with same id to DeferredRegister");
            }
            entries.Add(id);
        }

        /// <summary>
        /// 执行注册操作, 当 Registry<ET>.OnRegisterEvent 事件触发时被调用
        /// </summary>
        /// <param name="sender">no use</param>
        /// <param name="args">no use</param>
        private void DoRegister(object sender, RegisterEventArgs args)
        {
            /*LogUtil.Logger.Info($"DeferredRegister capture a register event:\n" +
                $"modid = ({_modid}), registry = ({_registry.RegEntryInfo.Id}),\n" +
                "entries = {");
            foreach (var pair in _entries)
            {
                LogUtil.Logger.Info(pair.Key.ToString());
            }
            LogUtil.Logger.Info("}");*/

            foreach (var id in entries)
            {
                registry.Register(id);
            }
            entries.Clear();
        }
    }
}
