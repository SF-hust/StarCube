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
        private readonly List<IEntry> entries = new List<IEntry>();

        private interface IEntry
        {
            public void RegisterTo(Registry<T> registry);

            public StringID Id { get; }
        }

        private class IdEntry : IEntry
        {
            public StringID Id => id;

            private readonly StringID id;

            public IdEntry(StringID id)
            {
                this.id = id;
            }

            public void RegisterTo(Registry<T> registry)
            {
                registry.Register(id);
            }
        }

        private class EntryEntry : IEntry
        {
            public StringID Id => id;

            private readonly StringID id;
            private readonly T entry;
            public EntryEntry(StringID id, T entry)
            {
                this.id = id;
                this.entry = entry;
            }

            public void RegisterTo(Registry<T> registry)
            {
                registry.Register(id, entry);
            }
        }
        
        public DeferredRegister(string modid, Registry<T> registry)
        {
            this.modid = modid;
            this.registry = registry;
            registry.OnRegisterStartEvent += DoRegister;
        }

        /// <summary>
        /// 向 DeferredRegister 中添加一个 name，用于新 entry 的命名
        /// </summary>
        /// <param name="name">name of registry entry</param>
        public void Register(string name)
        {
            if (Exist(name))
            {
                throw new InvalidOperationException($"can't add entry with same name ( = \"{name}\") to one DeferredRegister");
            }

            StringID id = StringID.Create(modid, name);
            entries.Add(new IdEntry(id));
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
            entries.Add(new EntryEntry(id, entry));
        }

        public bool Exist(string name)
        {
            foreach(IEntry entry in entries)
            {
                if(entry.Id.path == name)
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
            foreach (IEntry entry in entries)
            {
                entry.RegisterTo(registry);
            }
            entries.Clear();
            registry.OnRegisterStartEvent -= DoRegister;
        }
    }
}
