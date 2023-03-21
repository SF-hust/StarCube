using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using StarCube.Utility;
using StarCube.Core.Registry;

namespace StarCube.Core.Component
{
    public abstract class ComponentType<O> : IRegistryEntry<ComponentType<O>>
        where O : class, IComponentHolder<O>
    {
        /* ~ IRegistryEntry<ComponentType> 接口实现 start ~ */
        public RegistryEntryData<ComponentType<O>> RegistryEntryData
        {
            get => IRegistryEntry<ComponentType<O>>.RegistryEntryGetHelper(regData);
            set => IRegistryEntry<ComponentType<O>>.RegistryEntrySetHelper(ref regData, value);
        }
        private RegistryEntryData<ComponentType<O>>? regData = null;
        public virtual Type AsEntryType => typeof(ComponentType<O>);
        public Registry<ComponentType<O>> Registry => regData!.registry;
        public int IntegerID => regData!.integerID;
        public StringID ID => id;
        public string Modid => id.ModidString;
        public string Name => id.NameString;
        /* ~ IRegistryEntry<ComponentType> 接口实现 end ~ */


        /// <summary>
        /// 此 ComponentType 的 component 可附加到的类型
        /// </summary>
        public abstract Type OwnerType { get; }

        /// <summary>
        /// 各 Variants 对应组件的基类类型
        /// </summary>
        public abstract Type ComponentBaseType { get; }


        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public ComponentType(StringID id, bool allowMultiple)
        {
            this.id = id;
            this.allowMultiple = allowMultiple;
        }

        public readonly StringID id;

        public readonly bool allowMultiple;

    }

    /// <summary>
    /// Component 的类型
    /// </summary>
    /// <typeparam name="O">Owner 的类型</typeparam>
    /// <typeparam name="C">此类 ComponentType 对应的组件的基类型</typeparam>
    public class ComponentType<O, C> : ComponentType<O>
        where O : class, IComponentHolder<O>
        where C : IComponent<O>
    {
        public override Type OwnerType => typeof(O);
        public override Type ComponentBaseType => typeof(C);


        /// <summary>
        /// 所有注册到此 ComponentType 的 variant
        /// </summary>
        public IEnumerable<ComponentVariant<O, C>> Variants => idToVariants.Values;

        public void RegisterVariant(ComponentVariant<O, C> variant)
        {
            if (!idToVariants.TryAdd(variant.id, variant))
            {
                throw new Exception();
            }
        }


        public bool TryGetVariant(StringID variantId, [NotNullWhen(true)] out ComponentVariant<O, C>? variant)
        {
            return idToVariants.TryGetValue(variantId, out variant);
        }


        public ComponentType(StringID id, bool allowMultiple = false) : base(id, allowMultiple)
        {
            idToVariants = new ConcurrentDictionary<StringID, ComponentVariant<O, C>>();
        }

        private readonly ConcurrentDictionary<StringID, ComponentVariant<O, C>> idToVariants;
    }
}
