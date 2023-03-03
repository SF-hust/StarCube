using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using StarCube.Core.Registry;
using StarCube.Data;

namespace StarCube.Core.Component
{
    public abstract class ComponentType : IRegistryEntry<ComponentType>
    {
        /*
         * 作为 RegistryEntry
         */

        public RegistryEntryData<ComponentType> RegistryData
        {
            get => IRegistryEntry<ComponentType>.RegistryEntryGetHelper(regData);
            set => IRegistryEntry<ComponentType>.RegistryEntrySetHelper(ref regData, value);
        }
        private RegistryEntryData<ComponentType>? regData = null;

        public Type AsEntryType => typeof(ComponentType);

        /*
         * 作为 ComponentType
         */

        /// <summary>
        /// 此 ComponentType 的 component 可附加到的类型
        /// </summary>
        public abstract Type OwnerType { get; }

        /// <summary>
        /// 其中 Variants 的组件基类
        /// </summary>
        public abstract Type ComponentBaseType { get; }

        /// <summary>
        /// 所有注册到此 ComponentType 的组件列表
        /// </summary>
        public abstract IEnumerable<ComponentVariant> AbstractVariants { get; }

        public readonly StringID id;

        public ComponentType(StringID id)
        {
            this.id = id;
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }
    }

    /// <summary>
    /// Component 的类型
    /// </summary>
    /// <typeparam name="O">Owner 的类型</typeparam>
    /// <typeparam name="C">此类 Component 的基类型</typeparam>
    public class ComponentType<O, C> : ComponentType
        where O : class, IComponentHolder<O>
        where C : IComponent<O>
    {
        public override Type OwnerType => typeof(O);

        public override Type ComponentBaseType => typeof(C);

        public override IEnumerable<ComponentVariant> AbstractVariants => Variants;

        /// <summary>
        /// 所有注册到此 ComponentType 的组件列表
        /// </summary>
        public IEnumerable<ComponentVariant<O, C>> Variants => registeredVariants.Values;

        private readonly Dictionary<StringID, ComponentVariant<O, C>> registeredVariants = new Dictionary<StringID, ComponentVariant<O, C>>();

        public ComponentType(StringID id) : base(id)
        {
        }

        /// <summary>
        /// 注册一个 Variant
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="variantId"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        public ComponentVariant<O, C> Register<V>(StringID variantId, Func<V> factory)
            where V : C
        {
            return Register(variantId, () => factory(), typeof(V));
        }

        /// <summary>
        /// 非泛型版本的注册，需要自己手动填 underlyingType，并在 factory 中做类型转换
        /// </summary>
        /// <param name="variantId"></param>
        /// <param name="factory"></param>
        /// <param name="underlyingType"></param>
        /// <returns></returns>
        public ComponentVariant<O, C> Register(StringID variantId, Func<C> factory, Type underlyingType)
        {
            ComponentVariant<O, C> type = new ComponentVariant<O, C>(this, variantId, underlyingType, factory);
            registeredVariants.Add(variantId, type);
            return type;
        }

        public bool TryGetVariant(StringID variantId, [NotNullWhen(true)] out ComponentVariant<O, C>? variant)
        {
            return registeredVariants.TryGetValue(variantId, out variant);
        }
    }
}
