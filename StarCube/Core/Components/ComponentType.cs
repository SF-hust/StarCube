using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using StarCube.Utility;
using StarCube.Core.Registries;

namespace StarCube.Core.Components
{
    public abstract class ComponentType<O> : RegistryEntry<ComponentType<O>>
        where O : class, IComponentHolder<O>
    {
        /// <summary>
        /// 此 ComponentType 的 component 可附加到的类型
        /// </summary>
        public abstract Type OwnerType { get; }

        /// <summary>
        /// 各 Variants 对应组件的基类类型
        /// </summary>
        public abstract Type ComponentBaseType { get; }

        /// <summary>
        /// 尝试通过 id 获取 variant
        /// </summary>
        /// <param name="variantID"></param>
        /// <param name="variant"></param>
        /// <returns></returns>
        public abstract bool TryGetVariant(StringID variantID, [NotNullWhen(true)] out ComponentVariant<O>? variant);


        public ComponentType(Registry<ComponentType<O>> registry, StringID id, bool allowMultiple)
            : base(registry, id)
        {
            this.allowMultiple = allowMultiple;
        }

        public readonly bool allowMultiple;
    }

    /// <summary>
    /// Component 的类型
    /// </summary>
    /// <typeparam name="O">Owner 的类型</typeparam>
    /// <typeparam name="C">此类 ComponentType 对应的组件的基类型</typeparam>
    public class ComponentType<O, C> : ComponentType<O>
        where O : class, IComponentHolder<O>
        where C : Component<O>
    {
        public override Type OwnerType => typeof(O);
        public override Type ComponentBaseType => typeof(C);


        /// <summary>
        /// 所有注册到此 ComponentType 的 variant
        /// </summary>
        public IEnumerable<ComponentVariant<O, C>> Variants => idToVariants.Values;

        /// <summary>
        /// 注册一个 variant
        /// </summary>
        /// <param name="variant"></param>
        /// <exception cref="Exception"></exception>
        public void RegisterVariant(ComponentVariant<O, C> variant)
        {
            if (!idToVariants.TryAdd(variant.id, variant))
            {
                throw new Exception();
            }
        }

        public bool TryGetVariant(StringID variantID, [NotNullWhen(true)] out ComponentVariant<O, C>? variant)
        {
            return idToVariants.TryGetValue(variantID, out variant);
        }

        public override bool TryGetVariant(StringID variantID, [NotNullWhen(true)] out ComponentVariant<O>? variant)
        {
            if(TryGetVariant(variantID, out ComponentVariant<O, C>? vari))
            {
                variant = vari;
                return true;
            }

            variant = null;
            return false;
        }

        public ComponentType(Registry<ComponentType<O>> registry, StringID id, bool allowMultiple = false) : base(registry, id, allowMultiple)
        {
            idToVariants = new ConcurrentDictionary<StringID, ComponentVariant<O, C>>();
        }

        private readonly ConcurrentDictionary<StringID, ComponentVariant<O, C>> idToVariants;
    }
}
