using System;

using StarCube.Utility;

namespace StarCube.Core.Registry
{
    public interface IRegistryEntry : IStringID
    {
        public string Modid { get; }

        public string Name { get; }

        public int IntegerID { get; }

        public IRegistry Registry { get; }

        /// <summary>
        /// RegistryEntry 实际的类型
        /// </summary>
        public Type EntryType { get; }
    }

    public abstract class RegistryEntry<T> : IRegistryEntry
        where T : RegistryEntry<T>
    {
        public StringID ID => id;

        public string Modid => id.ModidString;

        public string Name => id.NameString;

        public int IntegerID
        {
            get => integerID;
            internal set => integerID = value;
        }

        public Registry<T> Registry => registry;


        /* ~ IRegistryEntry 接口实现 start ~ */
        IRegistry IRegistryEntry.Registry => registry;
        Type IRegistryEntry.EntryType => typeof(T);
        /* ~ IRegistryEntry 接口实现 end ~ */


        public RegistryEntry(Registry<T> registry, StringID id)
        {
            this.registry = registry;
            this.id = id;
        }

        private readonly Registry<T> registry;

        private readonly StringID id;

        private int integerID;
    }
}
