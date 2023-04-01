using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using StarCube.Utility;
using StarCube.Utility.Container;
using StarCube.Utility.Logging;

namespace StarCube.Core.Registry
{
    public interface IRegistry : IStringID
    {
        /// <summary>
        /// 此 Registry 内注册的 Entry 的实际类型
        /// </summary>
        public Type EntryType { get; }

        public IEnumerable<IRegistryEntry> Entries { get; }

        /// <summary>
        /// 尝试通过字符串 id 获取相应的 RegistryEntry
        /// </summary>
        /// <param name="id"></param>
        /// <param name="entry"></param>
        /// <returns></returns>
        public bool TryGetRegistryEntry(StringID id, [NotNullWhen(true)] out IRegistryEntry? entry);

        /// <summary>
        /// 尝试通过数字 id 获取相应的 RegistryEntry
        /// </summary>
        /// <param name="integerID"></param>
        /// <param name="entry"></param>
        /// <returns></returns>
        public bool TryGetRegistryEntry(int integerID, [NotNullWhen(true)] out IRegistryEntry? entry);

        /// <summary>
        /// 触发注册事件, 仅供游戏框架内部使用
        /// </summary>
        /// <returns>如果没有需要注册的 entry 了，返回 true，否则返回 false，此情况下这个函数会被再次调用直到 返回 true 为止</returns>
        public bool FireRegisterEvent();
    }

    /// <summary>
    /// 注册表类，保存了一系列的 RegistryEntry，可以向其注册游戏对象，或查找已注册的游戏对象
    /// </summary>
    /// <typeparam name="T">RegistryEntry 的类型</typeparam>
    public class Registry<T> : IRegistry, IIDMap<T>
        where T : RegistryEntry<T>
    {
        public static Registry<T> Create(string modid, string name)
        {
            return new Registry<T>(StringID.Create(modid, name));
        }


        /// <summary>
        /// 已被添加进此 Registry 中的 Entry 集合
        /// </summary>
        public IEnumerable<RegistryEntry<T>> Entries => entries;

        /// <summary>
        /// 尝试通过字符串 id 获取相应的 RegistryEntry
        /// </summary>
        /// <param name="id"></param>
        /// <param name="entry"></param>
        /// <returns></returns>
        public bool TryGetRegistryEntry(StringID id, [NotNullWhen(true)] out T? entry)
        {
            if (stringIDToIntegerID.TryGetValue(id, out int i))
            {
                entry = entries[i];
                return true;
            }

            entry = null;
            return false;
        }

        /// <summary>
        /// 尝试通过数字 id 获取相应的 RegistryEntry
        /// </summary>
        /// <param name="integerID"></param>
        /// <param name="entry"></param>
        /// <returns></returns>
        public bool TryGetRegistryEntry(int integerID, [NotNullWhen(true)] out T? entry)
        {
            if (integerID < entries.Count)
            {
                entry = entries[integerID];
                return true;
            }

            entry = null;
            return false;
        }

        /// <summary>
        /// 此 registry 是否已被锁定，被锁定的 registry 不能添加 entry
        /// </summary>
        public bool Locked => locked;

        public bool FireRegisterEvent()
        {
            LogUtil.Logger.Info($"start register event for registry \"{id}\"");

            RegisterStartEventArgs eventArg = new RegisterStartEventArgs();
            locked = false;
            OnRegisterStartEvent?.Invoke(this, eventArg);
            locked = true;
            return eventArg.isRegisterComplete;
        }

        /// <summary>
        /// 向 Registry 中添加新的 Entry
        /// </summary>
        /// <param name="entry"></param>
        /// <returns>如果此时 Registry 是 Lock 状态，则返回 false</returns>
        /// <exception cref="Exception">id 重复会导致异常</exception>
        public bool Register(T entry)
        {
            if (Locked)
            {
                return false;
            }

            if(stringIDToIntegerID.ContainsKey(entry.ID))
            {
                throw new Exception("entry");
            }

            int integerID = entries.Count;
            entry.IntegerID = integerID;

            entries.Add(entry);
            stringIDToIntegerID.Add(id, integerID);

            LogUtil.Logger.Info($"new entry \"{entry.ID}\" added to registry \"{id}\"");

            OnEntryAddEvent?.Invoke(this, new RegistryEntryAddEventArgs(entry));

            return true;
        }

        /// <summary>
        /// 获取 entry 的 RegistryObject 包装, 这个方法可以在 entry 被实际注册之前调用
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public RegistryObject<T> GetAsRegistryObject(StringID id)
        {
            return new RegistryObject<T>(this, id, () =>
            {
                if (TryGetRegistryEntry(id, out T? entry))
                {
                    return entry;
                }

                throw new ArgumentOutOfRangeException();
            });
        }


        /// <summary>
        /// 只有在此事件分发过程中才能向此 Registry 中添加 entry, 事件的 sender 是这个 Registry<T> 自身
        /// </summary>
        public event EventHandler<RegisterStartEventArgs>? OnRegisterStartEvent;

        /// <summary>
        /// 每当一个 entry 被添加到此 Registry，就会触发一次本事件
        /// </summary>
        public event EventHandler<EventArgs>? OnEntryAddEvent;

        public StringID ID => id;

        /* ~ IRegistry 接口实现 start ~ */
        Type IRegistry.EntryType => typeof(T);
        IEnumerable<IRegistryEntry> IRegistry.Entries => entries;
        bool IRegistry.TryGetRegistryEntry(StringID id, [NotNullWhen(true)] out IRegistryEntry? entry)
        {
            if(TryGetRegistryEntry(id, out T? e))
            {
                entry = e;
                return true;
            }

            entry = null;
            return false;
        }
        bool IRegistry.TryGetRegistryEntry(int integerID, [NotNullWhen(true)] out IRegistryEntry? entry)
        {
            if (TryGetRegistryEntry(integerID, out T? e))
            {
                entry = e;
                return true;
            }

            entry = null;
            return false;
        }
        /* ~ IRegistry 接口实现 end ~ */


        /* ~ IIDMap<T> 接口实现 start ~ */
        public int Count => entries.Count;
        int IIDMap<T>.IdFor(T value)
        {
            return value.IntegerID;
        }
        T IIDMap<T>.ValueFor(int id)
        {
            return entries[id];
        }
        /* ~ IIDMap<T> 接口实现 end ~ */


        /* IEnumrable<T> 接口实现 start */
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return entries.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<T>).GetEnumerator();
        }
        /* IEnumrable<T> 接口实现 end */



        public Registry(StringID id)
        {
            this.id = id;
            entries = new List<T>();
            stringIDToIntegerID = new Dictionary<StringID, int>();
            locked = true;
        }

        private readonly StringID id;

        private readonly List<T> entries;

        private readonly Dictionary<StringID, int> stringIDToIntegerID;

        private bool locked;
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
