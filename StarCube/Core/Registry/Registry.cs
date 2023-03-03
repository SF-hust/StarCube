using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using StarCube.Utility;
using StarCube.Utility.Container;
using StarCube.Data;

namespace StarCube.Core.Registry
{
    /// <summary>
    /// Registry 是注册表的抽象基类
    /// </summary>
    public abstract class Registry
    {
        /// <summary>
        /// 一个值为 "starcube:registry" 的 StringID，是每个 Registry 的 key.registry
        /// </summary>
        public static readonly StringID RegistryRegistry = StringID.Create(Constants.DEFAULT_NAMESPACE, Constants.REGISTRY_STRING);

        public Registry(StringID id)
        {
            this.id = id;
            key = StringKey.Create(RegistryRegistry, id);
        }

        /// <summary>
        /// 此 Registry 内注册的 Entry 的 C# 类型
        /// </summary>
        public abstract Type EntryType { get; }

        /// <summary>
        /// Registry 的 id
        /// </summary>
        public readonly StringID id;

        /// <summary>
        /// Registry 的 key
        /// </summary>
        public readonly StringKey key;

        /// <summary>
        /// 使用字符串 id 获取对应的数字 id
        /// </summary>
        /// <param name="location"></param>
        /// <returns>若对应的字符串 id 不存在返回 -1</returns>
        public abstract int GetNumIdByStringId(StringID location);

        /// <summary>
        /// 触发注册事件, 仅供游戏框架内部使用
        /// </summary>
        /// <returns>如果没有需要注册的 entry 了，返回 true，否则返回 false，此情况下这个函数会被再次调用直到 返回 true 为止</returns>
        public abstract bool FireRegisterEvent();
    }

    /// <summary>
    /// 注册表类，保存了一系列的 RegistryEntry，可以向其注册游戏对象，或查找已注册的游戏对象
    /// </summary>
    /// <typeparam name="T">RegistryEntry 的类型</typeparam>
    public class Registry<T> : Registry, IIdMap<T>, IEnumerable<T>
        where T : class, IRegistryEntry<T>
    {
        public static Registry<T> Create(string modid, string name)
        {
            return new Registry<T>(StringID.Create(modid, name));
        }

        public Registry(StringID id) : base(id)
        {
        }


        /* Registry 抽象类实现 start */
        public override Type EntryType => typeof(T);

        public override int GetNumIdByStringId(StringID location)
        {
            if (numIdByStringId.TryGetValue(location, out int id))
            {
                return id;
            }
            return -1;
        }

        public override bool FireRegisterEvent()
        {
            RegisterStartEventArgs eventArg = new RegisterStartEventArgs();
            isLocked = false;
            OnRegisterStartEvent?.Invoke(this, eventArg);
            isLocked = true;
            return eventArg.isRegisterComplete;
        }
        /* Registry 抽象类实现 end */



        /// <summary>
        /// 已被添加进此 Registry 中的 Entry 集合
        /// </summary>
        public IEnumerable<IRegistryEntry<T>> Entries => entries;

        protected readonly List<T> entries = new List<T>();

        protected readonly Dictionary<StringID, int> numIdByStringId = new Dictionary<StringID, int>();

        public bool TryGet(StringID id, [NotNullWhen(true)] out T? entry)
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

        public T Get(StringID id)
        {
            if (TryGet(id, out T? entry))
            {
                return entry;
            }
            throw new IndexOutOfRangeException();
        }
        
        /// <summary>
        /// 获取 entry 的 RegistryObject 包装, 这个方法可以在 entry 被实际注册之前调用
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public RegistryObject<T> GetAsRegistryObject(StringID id)
        {
            return new RegistryObject<T>(this, id, () => Get(id));
        }

        public bool IsLocked => isLocked;
        private bool isLocked = true;

        /// <summary>
        /// 向 Registry 中添加新的 Entry
        /// </summary>
        /// <param name="id"></param>
        /// <param name="entry"></param>
        /// <returns>如果此时 Registry 是 Lock 状态，则返回 false</returns>
        /// <exception cref="Exception">id 重复会导致异常</exception>
        public bool Register(StringID id, T entry)
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
            StringID registryId = this.id;
            RegistryEntryData<T> data = new RegistryEntryData<T>(numId, StringKey.Create(registryId, id), this, entry);
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


        /* IIdMap<T> 接口实现 start */
        public int Count => entries.Count;

        public int IdFor(T value)
        {
            return GetNumIdByStringId(value.Id);
        }

        public T ValueFor(int id)
        {
            return Get(id);
        }
        /* IIdMap<T> 接口实现 end */


        /* IEnumrable<T> 接口实现 start */
        public IEnumerator<T> GetEnumerator()
        {
            return entries.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        /* IEnumrable<T> 接口实现 end */
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

    /// <summary>
    /// Entry 注册事件的参数
    /// </summary>
    public class RegistryEntryAddEventArgs : EventArgs
    {
        public readonly IRegistryEntry registryEntry;

        public RegistryEntryAddEventArgs(IRegistryEntry registryEntry)
        {
            this.registryEntry = registryEntry;
        }
    }
}
