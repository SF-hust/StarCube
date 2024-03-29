﻿using System;
using System.Threading;

using StarCube.Utility;

namespace StarCube.Core.Registries
{
    /// <summary>
    /// 对 RegistryEntry 的 Lazy 包装, 通过此类对象可获取一个指定 id 的 T 类型的 RegistryEntry(也有可能不存在对应的 RegistryEntry 实例导致抛出异常)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RegistryObject<T> : Lazy<T>
        where T : RegistryEntry<T>
    {
        internal RegistryObject(Registry<T> registry, StringID id, Func<T> factory) : base(factory, LazyThreadSafetyMode.PublicationOnly)
        {
            this.registry = registry;
            this.id = id;
        }

        public readonly Registry<T> registry;

        public readonly StringID id;
    }
}
