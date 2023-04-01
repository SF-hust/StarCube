using System;
using System.Collections.Generic;

using StarCube.Utility;

namespace StarCube.Core.Registry
{
    /// <summary>
    /// 用于简化游戏对象的注册
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DeferredRegister<T>
        where T : RegistryEntry<T>
    {
        /// <summary>
        /// 创建一个指定 modid 的 DeferredRegister，后续注册时会使用这个 modid
        /// </summary>
        /// <param name="modid"></param>
        /// <returns></returns>
        public static DeferredRegister<T> Create(Registry<T> registry)
        {
            return new DeferredRegister<T>(registry);
        }


        /// <summary>
        /// 向 DeferredRegister 中添加一个新的 Entry
        /// </summary>
        /// <param name="entry"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void Register(T entry)
        {
            if (Contains(entry.ID))
            {
                throw new InvalidOperationException($"can't add entry with same id ( = \"{entry.ID}\") to one DeferredRegister");
            }

            entries.Add(entry);
        }

        /// <summary>
        /// 检查指定 id 的 registry entry 是否存在
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Contains(StringID id)
        {
            foreach(T entry in entries)
            {
                if(entry.ID.Equals(id))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 执行注册操作, 当 Registry<ET>.OnRegisterEvent 事件触发时被调用
        /// </summary>
        /// <param name="sender">no use</param>
        /// <param name="args">no use</param>
        private void DoRegister(object sender, RegisterStartEventArgs args)
        {
            foreach (T entry in entries)
            {
                registry.Register(entry);
            }
            entries.Clear();
            registry.OnRegisterStartEvent -= DoRegister;
        }

        public DeferredRegister(Registry<T> registry)
        {
            this.registry = registry;
            registry.OnRegisterStartEvent += DoRegister;
        }

        private readonly Registry<T> registry;
        private readonly List<T> entries = new List<T>();
    }
}
