using System;
using System.Collections.Generic;

namespace StarCube.Resource
{
    /// <summary>
    /// 表示一个资源在游戏中的 唯一Key，不会存在两个值相同的 ResourceKey 对象
    /// </summary>
    public sealed class ResourceKey : IComparable<ResourceKey>
    {
        /// <summary>
        /// 构成一个合法 ResourceKey 的字符串所需的最小长度
        /// </summary>
        public const int MIN_STRING_LENGTH = 7;

        /// <summary>
        /// 存储所有 ResourceKey, 以保证不会存在两个值相同的 ResourceKey 对象
        /// 此表可能会被多线程访问，所以创建对象时需要加锁，仅使用 Concurrent 版本无法保证不构造两个值相同的 ResourceKey 对象
        /// </summary>
        private static readonly Dictionary<string, ResourceKey> RESOURCE_KEYS = new Dictionary<string, ResourceKey>();

        public readonly ResourceLocation registry;
        public readonly ResourceLocation location;

        public readonly string keyString;

        private readonly int _hashcodeCache;

        /// <summary>
        /// 创建 ResourceKey，参数不合法则抛出异常
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        /// 虽然两个 ResourceLocation 均合法，但 registry.path 还需要符合 namespace 的格式，因此需要额外检查
        public static ResourceKey Create(ResourceLocation registry, ResourceLocation location)
        {
            ResourceKey? key = TryCreate(registry, location);
            if (key == null)
            {
                throw new ArgumentException($"Fail to create ResourceKey : registry.path \"{registry.path}\" is invalid for a ResourceKey");
            }
            return key;
        }

        /// <summary>
        /// 创建 ResourceKey，参数不合法则返回 null
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public static ResourceKey? TryCreate(ResourceLocation registry, ResourceLocation location)
        {
            // registry.path 需要符合 namespace 的格式
            if (!ResourceLocation.IsValidNamespace(registry.path))
            {
                return null;
            }
            return DoCreate(registry, location, null);
        }

        /// <summary>
        /// 从 "{registry}/{location}" 形式的字符串创建一个 ResourceLocation，如果参数不符合要求则抛出异常
        /// </summary>
        /// <param name="keyString"></param>
        /// <returns></returns>
        public static ResourceKey? Parse(string keyString)
        {
            ResourceKey? key = TryParse(keyString);
            if (key == null)
            {
                throw new ArgumentException($"Fail to parse \"{keyString}\" as ResourceKey");
            }
            return key;
        }

        /// <summary>
        /// 尝试从 "{registry}/{location}" 形式的字符串创建一个 ResourceLocation，如果参数不符合要求则返回 null
        /// </summary>
        /// <param name="keyString"></param>
        /// <returns></returns>
        public static ResourceKey? TryParse(string keyString)
        {
            string[] splits = keyString.Split('/', 2);
            if(splits.Length != 2)
            {
                return null;
            }
            ResourceLocation? registry = ResourceLocation.TryParse(splits[0]);
            if(registry == null)
            {
                return null;
            }
            // registry.path 需要符合 namespace 的格式
            if (!ResourceLocation.IsValidNamespace(registry.path))
            {
                return null;
            }
            ResourceLocation? location = ResourceLocation.TryParse(splits[1]);
            if (location == null)
            {
                return null;
            }
            return DoCreate(registry, location, keyString);
        }

        private static ResourceKey DoCreate(ResourceLocation registry, ResourceLocation location, string? keyString)
        {
            // 来自类内部的调用不会传入与 registry 和 location 不匹配的 keyString
            keyString ??= registry.ToString() + "/" + location.ToString();

            ResourceKey resourceKey;
            // 这里加锁是为了保证不创建值相同的 ResourceKey 对象，Concurrent 版本在这里是没法起作用的
            lock (RESOURCE_KEYS)
            {
                if (!RESOURCE_KEYS.TryGetValue(keyString, out resourceKey))
                {
                    resourceKey = new ResourceKey(registry, location, keyString);
                    RESOURCE_KEYS.TryAdd(keyString, resourceKey);
                }
            }

            return resourceKey;
        }

        private ResourceKey(ResourceLocation registry, ResourceLocation location, string keyString)
        {
            this.registry = registry;
            this.location = location;
            this.keyString = keyString;
            _hashcodeCache = 31 * registry.GetHashCode() + location.GetHashCode();
        }

        public int CompareTo(ResourceKey? other)
        {
            int rns = registry.CompareTo(other?.registry);
            if (rns != 0)
            {
                return rns;
            }
            return location.CompareTo(other?.location);
        }

        /// <summary>
        /// 仅需比较引用即可判断是否相等
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            return object.ReferenceEquals(this, obj);
        }

        public override int GetHashCode()
        {
            return _hashcodeCache;
        }

        /// <summary>
        /// 返回一个 "{registry}/{location}" 形式的字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return keyString;
        }
    }
}
