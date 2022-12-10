using System;
using System.Collections.Generic;

namespace StarCube.Resource
{
    /// <summary>
    /// 表示一个资源在游戏中的 唯一Key，不会存在两个值相同的 ResourceKey 对象
    /// </summary>
    /// 
    public sealed class ResourceKey : IComparable<ResourceKey>
    {
        /// <summary>
        /// 存储所有 ResourceKey, 以保证不会存在两个值相同的 ResourceKey 对象
        /// 此表可能会被多线程访问，所以创建对象时需要加锁，仅使用 Concurrent 版本无法保证不构造两个值相同的 ResourceKey 对象
        /// </summary>
        private static readonly Dictionary<string, ResourceKey> RESOURCE_KEYS = new Dictionary<string, ResourceKey>();

        public readonly ResourceLocation registry;
        public readonly ResourceLocation location;

        private readonly int _hashcodeCache;

        /// <summary>
        /// 创建 ResourceKey，由于是从两个合法的 ResourceLocation 中创建的，所以无需检查
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public static ResourceKey Create(ResourceLocation registry, ResourceLocation location)
        {
            string key = registry.ToString() + "/" + location.ToString();
            ResourceKey resourceKey;
            // 这里加锁是为了保证不创建值相同的 ResourceKey 对象，Concurrent 版本在这里是没法起作用的
            lock (RESOURCE_KEYS)
            {
                if (!RESOURCE_KEYS.TryGetValue(key, out resourceKey))
                {
                    resourceKey = new ResourceKey(registry, location);
                    RESOURCE_KEYS.TryAdd(key, resourceKey);
                }
            }
            return resourceKey;
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
            ResourceLocation? location = ResourceLocation.TryParse(splits[1]);
            if (location == null)
            {
                return null;
            }
            return Create(registry, location);
        }

        private ResourceKey(ResourceLocation registry, ResourceLocation location)
        {
            this.registry = registry;
            this.location = location;
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
            return registry.ToString() + "/" + location.ToString();
        }
    }
}
