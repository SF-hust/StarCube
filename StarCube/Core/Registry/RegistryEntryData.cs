using System;
using StarCube.Data;

namespace StarCube.Core.Registry
{
    /// <summary>
    /// 无类型的 RegistryEntry 注册数据
    /// </summary>
    public abstract class RegistryEntryData
    {
        protected RegistryEntryData(int numID, StringKey key)
        {
            this.numID = numID;
            this.key = key;
        }

        public readonly int numID;
        /// <summary>
        /// 此 Entry 在 Registry 中的数字Id, 在同一个 Registry 中唯一
        /// </summary>

        /// <summary>
        /// 此 Entry 的 ResourceKey, 在整个游戏中唯一
        /// </summary>
        public readonly StringKey key;

        /// <summary>
        /// 此 Entry 的字符串 Id, 即 "namespace:name", 在同一个 Registry 中唯一
        /// </summary>
        public StringID ID => key.location;

        /// <summary>
        /// 此 Entry 的 modid
        /// </summary>
        public string Modid => ID.namspace;

        /// <summary>
        /// 此 Entry 的名字, 在本模组命名空间下, 同一个 Registry 中唯一
        /// </summary>
        public string Name => key.location.path;

        /// <summary>
        /// 此 Entry 的具体类型
        /// </summary>
        public abstract Type EntryType { get; }

        /// <summary>
        /// 此 Entry 被注册到的 Registry 实例, 其中的 Entry 类型未知
        /// </summary>
        public abstract Registry AbstractRegistry { get; }

        /// <summary>
        /// 实际 RegistryEntry 的引用, 没有具体类型信息
        /// </summary>
        public abstract IRegistryEntry AbstractEntry { get; }

        /// <summary>
        /// RegistryEntryInfo 的 hashcode 被定义为其 ResourceKey 的 hashcode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return key.GetHashCode();
        }

        /// <summary>
        /// RegistryEntryInfo 不会出现两个不同实例的值相同的情况
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj);
        }
    }

    /// <summary>
    /// 已被注册到 Registry 中的 Entry 的信息数据
    /// </summary>
    /// <typeparam name="T"> RegistryEntry 的具体类型 </typeparam>
    /// 当 RegistryEntry 被添加到 Registry 中时，会创建对应的此类示例
    public class RegistryEntryData<T> : RegistryEntryData
        where T : class, IRegistryEntry<T>
    {
        public RegistryEntryData(int numID, StringKey key, Registry<T> registry, T entry) : base(numID, key)
        {
            this.registry = registry;
            this.entry = entry;
        }

        public override Type EntryType => typeof(T);

        public override Registry AbstractRegistry => Registry;

        public override IRegistryEntry AbstractEntry => Entry;

        /// <summary>
        /// 此 Entry 被注册到的 Registry
        /// </summary>
        public Registry<T> Registry => registry;

        protected Registry<T> registry;

        /// <summary>
        /// 实际 RegistryEntry 对象的引用
        /// </summary>
        public T Entry => entry;
        protected T entry;

        public override string ToString()
        {
            return $"RegistryEntryData (type = {EntryType}, registry id = {Registry.id}, num id = {numID}, id = {ID})";
        }
    }
}
