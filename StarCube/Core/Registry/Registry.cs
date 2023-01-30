using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using StarCube.Utility;
using StarCube.Utility.Container;
using StarCube.Resource;

namespace StarCube.Core.Registry
{
    /// <summary>
    /// Registry 是注册表的抽象基类, 可以向其注册游戏对象(此操作一般由 DefferedRegister 完成), 或查找已注册的游戏对象
    /// </summary>
    public abstract class Registry
    {
        public static readonly ResourceLocation RegistryRegistry = ResourceLocation.Create(Constants.DEFAULT_NAMESPACE, Constants.REGISTRY_STRING);

        /// <summary>
        /// 此 Registry 内注册的 Entry 的具体类型
        /// </summary>
        public abstract Type EntryType { get; }

        /// <summary>
        /// Registry 的 id
        /// </summary>
        public readonly ResourceLocation id;

        public readonly ResourceKey key;

        public Registry(ResourceLocation id)
        {
            this.id = id;
            key = ResourceKey.Create(RegistryRegistry, id);
        }

        /// <summary>
        /// 使用 字符串id 获取对应的 数字id
        /// </summary>
        /// <param name="location"></param>
        /// <returns>若对应的 字符串id 不存在返回 -1</returns>
        public abstract int GetNumIdByStringId(ResourceLocation location);

        /// <summary>
        /// 触发注册事件, 仅供游戏框架内部使用
        /// </summary>
        /// <returns>如果没有需要注册的 entry 了，返回 true，否则返回 false，这样这个函数还会被再次调用</returns>
        public abstract bool FireRegisterEvent();
    }

    /// <summary>
    /// 注册表类，保存了一系列的 RegistryEntry，可以向其注册游戏对象，或查找已注册的游戏对象
    /// </summary>
    /// <typeparam name="T">RegistryEntry 的类型</typeparam>
    public class Registry<T> : Registry, IIdMap<T>
        where T : class, IRegistryEntry<T>
    {
        protected readonly List<T> entries = new List<T>();

        protected readonly Dictionary<ResourceLocation, int> numIdByStringId = new Dictionary<ResourceLocation, int>();

        protected readonly Func<T>? entryFactory;

        /// <summary>
        /// 通过此方法创建一个 Registry
        /// </summary>
        /// <param name="modid"></param>
        /// <param name="name"></param>
        /// <param name="entryFactory"></param>
        /// <returns></returns>
        public static Registry<T> Create(string modid, string name, Func<T>? entryFactory = null)
        {
            return new Registry<T>(ResourceLocation.Create(modid, name), entryFactory);
        }

        public Registry(ResourceLocation id, Func<T>? entryFactory) : base(id)
        {
            this.entryFactory = entryFactory;
        }

        public override Type EntryType => typeof(T);

        /// <summary>
        /// 已被添加进此 Registry 中的 Entry 集合
        /// </summary>
        public IEnumerable<IRegistryEntry<T>> Entries => entries;

        public bool TryGet(ResourceLocation id, [NotNullWhen(true)] out T? entry)
        {
            if (numIdByStringId.TryGetValue(id, out int i))
            {
                entry = entries[i];
                return true;
            }
            entry = null;
            return false;
        }

        public bool TryGet(int numId, [NotNullWhen(true)] out T? entry)
        {
            if (numId < entries.Count)
            {
                entry = entries[numId];
                return true;
            }
            entry = null;
            return false;
        }

        public T Get(int numId)
        {
            if(TryGet(numId, out T? entry))
            {
                return entry;
            }
            throw new IndexOutOfRangeException();
        }

        public T Get(ResourceLocation id)
        {
            if (TryGet(id, out T? entry))
            {
                return entry;
            }
            throw new IndexOutOfRangeException();
        }

        public override int GetNumIdByStringId(ResourceLocation location)
        {
            if (numIdByStringId.TryGetValue(location, out int id))
            {
                return id;
            }
            return -1;
        }

        public bool IsLocked => isLocked;
        private bool isLocked = true;

        public int Count => entries.Count;

        /// <summary>
        /// 构造一个默认 entry 对象并注册
        /// </summary>
        /// <param name="id"></param>
        /// <returns>如果 Registry 不知道如何创建 entry 对象，或此时 Registry 是 Lock 状态，则返回 false</returns>
        /// <exception cref="Exception">id 重复会导致异常</exception>
        public bool Register(ResourceLocation id)
        {
            if (entryFactory == null)
            {
                return false;
            }
            return Register(id, entryFactory());
        }

        /// <summary>
        /// 向 Registry 中添加新的 Entry
        /// </summary>
        /// <param name="id"></param>
        /// <param name="entry"></param>
        /// <returns>如果此时 Registry 是 Lock 状态，则返回 false</returns>
        /// <exception cref="Exception">id 重复会导致异常</exception>
        public bool Register(ResourceLocation id, T entry)
        {
            if (IsLocked)
            {
                return false;
            }
            if(numIdByStringId.ContainsKey(id))
            {
                throw new Exception("");
            }

            int numId = entries.Count;
            ResourceLocation registryId = this.id;
            RegistryEntryData<T> data = new RegistryEntryData<T>(numId, ResourceKey.Create(registryId, id), this, entry);
            entry.RegistryData = data;

            entries.Add(entry);
            numIdByStringId.Add(id, numId);

            OnEntryAddEvent?.Invoke(this, new RegistryEntryAddEventArgs(entry));

            return true;
        }

        /// <summary>
        /// 只有在此事件分发过程中才能向此 Registry 中添加 entry, 事件的 sender 是这个 Registry<T> 自身
        /// </summary>
        public event EventHandler<RegisterStartEventArgs>? OnRegisterStartEvent;

        /// <summary>
        /// 每当一个 entry 被添加到此 Registry，就会触发一次本事件
        /// </summary>
        public event EventHandler<EventArgs>? OnEntryAddEvent;

        public override bool FireRegisterEvent()
        {
            RegisterStartEventArgs eventArg = new RegisterStartEventArgs();
            isLocked = false;
            OnRegisterStartEvent?.Invoke(this, eventArg);
            isLocked = true;
            return eventArg.isRegisterComplete;
        }

        /// <summary>
        /// 获取 entry 的 RegistryObject 包装, 这个方法可以在 entry 被实际注册之前调用
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public RegistryObject<T> GetAsRegistryObject(ResourceLocation id)
        {
            return new RegistryObject<T>(this, id, () => Get(id));
        }

        public int IdFor(T value)
        {
            return GetNumIdByStringId(value.Id);
        }

        public T ValueFor(int id)
        {
            return Get(id);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return entries.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /// <summary>
    /// Registry 事件的参数
    /// </summary>
    public class RegisterStartEventArgs : EventArgs
    {
        public bool isRegisterComplete = true;

        public RegisterStartEventArgs() : base()
        {
        }
    }

    public class RegistryEntryAddEventArgs : EventArgs
    {
        public readonly IRegistryEntry registryEntry;

        public RegistryEntryAddEventArgs(IRegistryEntry registryEntry)
        {
            this.registryEntry = registryEntry;
        }
    }
}
