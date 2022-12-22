using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using StarCube.Resource;

namespace StarCube.Core.Registry
{
    /// <summary>
    /// Registry 是一个注册表, 可以向其注册游戏对象(此操作一般由 DefferedRegister 完成), 或查找已注册的游戏对象
    /// </summary>
    public abstract class Registry
    {
        /// <summary>
        /// 此 Registry 内注册的 Entry 的具体类型
        /// </summary>
        public abstract Type EntryType { get; }

        /// <summary>
        /// Registry 的 id
        /// </summary>
        public readonly ResourceLocation id;

        public Registry(ResourceLocation id)
        {
            this.id = id;
        }

        /// <summary>
        /// 使用 字符串id 获取对应的 数字id
        /// </summary>
        /// <param name="location"></param>
        /// <returns>若对应的 字符串id 不存在返回 -1</returns>
        public abstract int GetNumIdByStringId(ResourceLocation location);

        /// <summary>
        /// 触发注册事件, Modder 不应使用该方法
        /// </summary>
        public abstract bool FireRegisterEvent();
    }

    /// <summary>
    /// 注册表, 保存了一系列的 RegistryEntry
    /// </summary>
    /// <typeparam name="T">RegistryEntry 的类型</typeparam>
    public class Registry<T> : Registry
        where T : class, IRegistryEntry<T>
    {
        protected readonly List<T> entries = new List<T>();

        protected readonly Dictionary<ResourceLocation, int> numIdByStringId = new Dictionary<ResourceLocation, int>();

        protected readonly Func<T> entryFactory;

        public Registry(string modid, string name, Func<T> entryFactory) : base(ResourceLocation.Create(modid, name))
        {
            this.entryFactory = entryFactory;
        }

        public override Type EntryType => typeof(T);

        /// <summary>
        /// 已被添加进此 Registry 中的 Entry 集合
        /// </summary>
        public IEnumerable<IRegistryEntry<T>> Entries => entries;

        /// <summary>
        /// 通过 数字id 获取某个 RegistryEntry
        /// </summary>
        /// <param name="numId">entry 的 数字id</param>
        /// <param name="entry">entry 实体</param>
        /// <returns></returns>
        public bool TryGetEntryByNumId(int numId, out T? entry)
        {
            if (numId < entries.Count)
            {
                entry = entries[numId];
                return true;
            }
            entry = null;
            return false;
        }

        /// <summary>
        /// 通过 字符串id 获取某个 RegistryEntry
        /// </summary>
        /// <param name="id"></param>
        /// <param name="entry"></param>
        /// <returns>true if entry of location exists, or else false</returns>
        public bool TryGetEntryById(ResourceLocation id, [NotNullWhen(true)] out T? entry)
        {
            if (numIdByStringId.TryGetValue(id, out int i))
            {
                entry = entries[i];
                return true;
            }
            entry = null;
            return false;
        }

        public override int GetNumIdByStringId(ResourceLocation location)
        {
            if (numIdByStringId.TryGetValue(location, out int id))
            {
                return id;
            }
            return -1;
        }

        private bool isLocked = false;

        /// <summary>
        /// Registry 是否已锁定, 已锁定的 Registry 不能被添加新 Entry, 注册事件完成后, Registry 会被锁定
        /// </summary>
        public bool IsLocked => isLocked;

        /// <summary>
        /// 锁定此 Registry, Modder不应调用这个方法
        /// </summary>
        public void LockRegistry()
        {
            isLocked = true;
        }

        public bool Register(ResourceLocation id)
        {
            return Add(entryFactory(), id);
        }

        /// <summary>
        /// 向 Registry 中添加新的 Entry
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        /// 注意是直接添加，注册不用这个方法
        public bool Add(T entry, ResourceLocation id)
        {
            if (IsLocked)
            {
                return false;
            }
            int numId = entries.Count;
            entries.Add(entry);
            ResourceLocation registryLocation = this.id;
            RegistryEntryData<T> data = new RegistryEntryData<T>(numId, ResourceKey.Create(registryLocation, id), this, entry);
            entry.RegistryData = data;
            //LogUtil.Logger.Info($"a new entry added to registry ({RegEntryInfo.Id}) :\n" +
            //    $"id = {id}, location = {location}");
            return true;
        }

        /// <summary>
        /// Registry 事件的参数
        /// </summary>
        public class RegisterEventArgs : EventArgs
        {
            /// <summary>
            /// 空的 Registry 事件参数
            /// </summary>
            public static new RegisterEventArgs Empty = new RegisterEventArgs(string.Empty);

            public readonly string modid;

            public bool IsRegisterComplete = true;

            private RegisterEventArgs(string modid) : base()
            {
                this.modid = modid;
            }
        }

        /// <summary>
        /// 注册事件, 注册游戏对象时该事件会被触发, 事件的触发者将是这个 Registry 自身
        /// </summary>
        public event EventHandler<RegisterEventArgs>? OnRegisterEvent;

        public override bool FireRegisterEvent()
        {
            OnRegisterEvent?.Invoke(this, RegisterEventArgs.Empty);
            return RegisterEventArgs.Empty.IsRegisterComplete;
        }

        /// <summary>
        /// 获取指定 id 的 Entry 的 RegistryObject 包装, 这个方法可以在注册事件发生之前调用
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public RegistryObject<T> GetEntryAsRegistryObject(ResourceLocation id)
        {
            return new RegistryObject<T>(() =>
            {
                return TryGetEntryById(id, out var entry) ? entry : null;
            });
        }
    }
}
