using System;
using System.Collections.Generic;

using StarCube.Resource;

namespace StarCube.Core.Registry
{
    /// <summary>
    /// 用于简化游戏对象的注册
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DeferredRegister<T>
        where T : class, IRegistryEntry<T>
    {
        /// <summary>
        /// 创建一个指定 modid 的 DeferredRegister，后续注册时会使用这个 modid
        /// </summary>
        /// <param name="modid"></param>
        /// <returns></returns>
        public static DeferredRegister<T> Create(Registry<T> registry, string modid)
        {
            return new DeferredRegister<T>(modid, registry);
        }

        private readonly string modid;
        private readonly Registry<T> registry;
        private readonly List<KeyValuePair<StringID, T>> entries = new List<KeyValuePair<StringID, T>>();

        public DeferredRegister(string modid, Registry<T> registry)
        {
            this.modid = modid;
            this.registry = registry;
            registry.OnRegisterStartEvent += DoRegister;
        }

        /// <summary>
        /// 向 DeferredRegister 中添加一个新的 Entry
        /// </summary>
        /// <param name="name"></param>
        /// <param name="entry"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void Register(string name, T entry)
        {
            if (Exist(name))
            {
                throw new InvalidOperationException($"can't add entry with same name ( = \"{name}\") to one DeferredRegister");
            }

            StringID id = StringID.Create(modid, name);
            entries.Add(new KeyValuePair<StringID, T>(id, entry));
        }

        public bool Exist(string name)
        {
            foreach(var pair in entries)
            {
                if(pair.Key.path == name)
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
            foreach (var pair in entries)
            {
                registry.Register(pair.Key, pair.Value);
            }
            entries.Clear();
            registry.OnRegisterStartEvent -= DoRegister;
        }
    }
}
