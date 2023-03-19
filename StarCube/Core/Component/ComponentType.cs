using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using StarCube.Utility;
using StarCube.Core.Registry;

namespace StarCube.Core.Component
{
    public abstract class ComponentType : IRegistryEntry<ComponentType>
    {
        /* ~ IRegistryEntry<ComponentType> 接口实现 start ~ */
        public RegistryEntryData<ComponentType> RegistryEntryData
        {
            get => IRegistryEntry<ComponentType>.RegistryEntryGetHelper(regData);
            set => IRegistryEntry<ComponentType>.RegistryEntrySetHelper(ref regData, value);
        }
        public virtual Type AsEntryType => typeof(ComponentType);
        public Registry<ComponentType> Registry => regData!.registry;
        public StringID ID => regData!.id;
        public int IntegerID => regData!.integerID;
        public string Modid => regData!.Modid;
        public string Name => regData!.Name;
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

        private RegistryEntryData<ComponentType>? regData = null;
    }

    /// <summary>
    /// Component 的类型
    /// </summary>
    /// <typeparam name="O">Owner 的类型</typeparam>
    /// <typeparam name="C">此类 ComponentType 对应的组件的基类型</typeparam>
    public class ComponentType<O, C> : ComponentType
        where O : class, IComponentHolder<O>
        where C : IComponent<O>
    {
        public override Type OwnerType => typeof(O);
        public override Type ComponentBaseType => typeof(C);


        /// <summary>
        /// 所有注册到此 ComponentType 的 variant
        /// </summary>
        public IEnumerable<ComponentVariant<O, C>> Variants => idToVariants.Values;


        /// <summary>
        /// 注册一个 Variant
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="variantID"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        public ComponentVariant<O, C> Register<V>(StringID variantID, ComponentFactory<O, C> factory)
            where V : C
        {
            return Register(variantID, factory, typeof(V));
        }

        /// <summary>
        /// 非泛型版本的注册，需要自己手动填 underlyingType，并在 factory 中做类型转换
        /// </summary>
        /// <param name="variantID"></param>
        /// <param name="factory"></param>
        /// <param name="underlyingType"></param>
        /// <returns></returns>
        public ComponentVariant<O, C> Register(StringID variantID, ComponentFactory<O, C> factory, Type underlyingType)
        {
            ComponentVariant<O, C> type = new ComponentVariant<O, C>(this, variantID, underlyingType, factory);

            if(!idToVariants.TryAdd(variantID, type))
            {
                throw new Exception();
            }

            return type;
        }

        public bool TryGetVariant(StringID variantId, [NotNullWhen(true)] out ComponentVariant<O, C>? variant)
        {
            return idToVariants.TryGetValue(variantId, out variant);
        }


        public ComponentType(StringID id, bool allowMultiple = false) : base(id, allowMultiple)
        {
        }

        private readonly ConcurrentDictionary<StringID, ComponentVariant<O, C>> idToVariants = new ConcurrentDictionary<StringID, ComponentVariant<O, C>>();
    }
}
