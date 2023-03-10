using System;

using StarCube.Utility;

namespace StarCube.Core.Registry
{
    public interface IRegistryEntry : IStringID, IIntegerID
    {
        /// <summary>
        /// 具体类型未知的 RegistryEntry 信息
        /// </summary>
        public RegistryEntryData AbstractEntryRegistryData { get; }

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
        public StringKey Key => AbstractEntryRegistryData.key;

        /// <summary>
        /// 该对象的字符串 id
        /// </summary>
        public new StringID ID => AbstractEntryRegistryData.ID;

        /// <summary>
        /// 该对象的整数 id
        /// </summary>
        public new int IntegerID => AbstractEntryRegistryData.integerID;

        /// <summary>
        /// 该对象的名称
        /// </summary>
        public string Name => AbstractEntryRegistryData.Name;

        /// <summary>
        /// 该对象所属的 mod 的 modid
        /// </summary>
        public string Modid => AbstractEntryRegistryData.Modid;

        /// <summary>
        /// 该对象的 Registry, 具体类型未知
        /// </summary>
        public Registry AbstractRegistry => AbstractEntryRegistryData.AbstractRegistry;

        StringID IStringID.ID => ID;

        int IIntegerID.IntegerID => IntegerID;
    }

    /// <summary>
    /// 所有需要被注册到 Registry 的对象的类应实现该接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRegistryEntry<T> : IRegistryEntry
        where T : class, IRegistryEntry<T>
    {
        /*
         * 对本接口的实现应为如下形式 :
         * 
         * public class T : IRegistryEntry<T>
         * {
         *     public RegistryEntryData<T> RegistryData
         *     {
         *         get => IRegistryEntry<T>.RegistryEntryGetHelper(regData);
         *         set => IRegistryEntry<T>.RegistryEntrySetHelper(ref regData, value);
         *     }
         *     private RegistryEntryData<T>? regData = null;
         *     public Type AsEntryType => typeof(T);
         *     ...
         * }
         * 
         * 将其中的 T 换成你自己的 RegistryEntry 的类型
         */

        protected static RegistryEntryData<T> RegistryEntryGetHelper(RegistryEntryData<T>? data)
        {
            return data ?? throw new NullReferenceException($"RegistryEntry (type = {typeof(T).FullName}) has not been constructed");
        }

        protected static void RegistryEntrySetHelper(ref RegistryEntryData<T>? data, RegistryEntryData<T> value)
        {
            data ??= value;
        }

        /// <summary>
        /// RegistryEntry 的信息
        /// </summary>
        public RegistryEntryData<T> RegistryEntryData { get; set; }

        /*
         * 无需 override 的默认实现
         */

        RegistryEntryData IRegistryEntry.AbstractEntryRegistryData => RegistryEntryData;

        public Registry<T> Registry => RegistryEntryData.Registry;
    }
}
