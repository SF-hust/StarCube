using System;

using StarCube.Resource;

namespace StarCube.Core.Registry
{
    public interface IRegistryEntry
    {
        /// <summary>
        /// 具体类型未知的 RegistryEntry 信息
        /// </summary>
        public RegistryEntryData AbstractRegistryData { get; }

        /// <summary>
        /// RegistryEntry的具体类型, 如 Block
        /// </summary>
        public Type AsEntryType { get; }

        /*
         * 无需 override 的默认实现
         */

        /// <summary>
        /// 该对象注册用的 Key
        /// </summary>
        public StringKey Key => AbstractRegistryData.RegKey;

        /// <summary>
        /// 该对象的 字符串id
        /// </summary>
        public StringID Id => AbstractRegistryData.Id;

        /// <summary>
        /// 该对象的名称
        /// </summary>
        public string Name => AbstractRegistryData.Name;

        /// <summary>
        /// 该对象所属的 modid
        /// </summary>
        public string ModId => AbstractRegistryData.ModId;

        /// <summary>
        /// 该对象的 数字id
        /// </summary>
        public int NumId => AbstractRegistryData.NumId;

        /// <summary>
        /// 该对象的 Registry, 具体类型未知
        /// </summary>
        public Registry AbstractRegistry => AbstractRegistryData.AbstractRegistry;
    }

    /// <summary>
    /// 所有需要被注册到 Registry 的对象的类应实现该接口,
    /// 并实现其中的属性
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// 游戏中，一个 RegistryEntry 的生命周期如下：
    /// 1.预注册，模组加载器读取模组数据，获取每个模组想注册的 Entry 的信息
    /// 2.注册与实例化，注册事件开始后，模组
    public interface IRegistryEntry<T> : IRegistryEntry
        where T : class, IRegistryEntry<T>
    {
        /// <summary>
        /// RegistryEntry 的信息, 包括注册名, Registry 实例的引用, Entry 实例的引用等
        /// </summary>
        public RegistryEntryData<T> RegistryData { get; set; }

        /*
         * 无需 override 的默认实现
         */

        RegistryEntryData IRegistryEntry.AbstractRegistryData => RegistryData;

        public Registry<T> Registry => RegistryData.Registry;
    }
}
