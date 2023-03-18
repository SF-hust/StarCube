using System;

using StarCube.Utility;

namespace StarCube.Core.Registry
{
    /// <summary>
    /// 无类型的 RegistryEntry 注册数据
    /// </summary>
    public abstract class RegistryEntryData
    {
        public string Modid => id.ModidString;
        public string Name => id.NameString;

        /// <summary>
        /// 此 Entry 的具体类型
        /// </summary>
        public abstract Type EntryType { get; }

        /// <summary>
        /// 此 Entry 被注册到的 Registry 实例
        /// </summary>
        public abstract Registry AbstractRegistry { get; }

        /// <summary>
        /// 实际 RegistryEntry 的引用
        /// </summary>
        public abstract IRegistryEntry AbstractEntry { get; }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public override string ToString()
        {
            return id.ToString();
        }

        public RegistryEntryData(int integerID, StringID id)
        {
            this.integerID = integerID;
            this.id = id;
        }

        public readonly int integerID;

        public readonly StringID id;
    }

    /// <summary>
    /// 已被注册到 Registry 中的 Entry 的信息数据
    /// </summary>
    /// <typeparam name="T"> RegistryEntry 的具体类型 </typeparam>
    /// 当 RegistryEntry 被添加到 Registry 中时，会创建对应的此类示例
    public class RegistryEntryData<T> : RegistryEntryData
        where T : class, IRegistryEntry<T>
    {
        public override Type EntryType => typeof(T);
        public override Registry AbstractRegistry => registry;
        public override IRegistryEntry AbstractEntry => entry;


        public RegistryEntryData(int integerID, StringID id, Registry<T> registry, T entry) : base(integerID, id)
        {
            this.registry = registry;
            this.entry = entry;
        }

        public readonly Registry<T> registry;

        public readonly T entry;
    }
}
