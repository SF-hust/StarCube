﻿using System;
using System.Collections.Generic;
using System.Text;

using StarCube.Utility;

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


        public const char REGISTRY_SEPARATOR = '/';

        public static readonly ResourceKey Failed = DoCreate("_", "_", "_", "_", "_:_/_:_");


        /// <summary>
        /// 存储所有 ResourceKey, 以保证不会存在两个值相同的 ResourceKey 对象
        /// 此表可能会被多线程访问，所以创建对象时需要加锁，仅使用 Concurrent 版本无法保证不构造两个值相同的 ResourceKey 对象
        /// </summary>
        private static readonly Dictionary<string, ResourceKey> RESOURCE_KEYS = new Dictionary<string, ResourceKey>();


        public readonly ResourceLocation registry;
        public readonly ResourceLocation location;


        /// <summary>
        /// 字符串形式的 Key
        /// </summary>
        public readonly string keyString;


        /// <summary>
        /// 由两个 StringID 创建一个 ResourceKey，参数不合法则抛出异常
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        /// 虽然两个 ResourceLocation 均合法，但 registry.path 还需要符合 namespace 的格式，因此需要额外检查
        public static ResourceKey Create(ResourceLocation registry, ResourceLocation location)
        {
            if (!TryCreate(registry, location, out ResourceKey key))
            {
                throw new ArgumentException($"Fail to create ResourceKey : registry.path \"{registry.path}\" is invalid for a ResourceKey");
            }
            return key;
        }


        /// <summary>
        /// 由四个字符串创建一个 ResourceKey，参数不合法则抛出异常
        /// </summary>
        /// <param name="regSpace"></param>
        /// <param name="regName"></param>
        /// <param name="idSpace"></param>
        /// <param name="idName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static ResourceKey Create(string regSpace, string regName, string idSpace, string idName)
        {
            if(!TryCreate(regSpace, regName, idSpace, idName, out ResourceKey key))
            {
                throw new ArgumentException($"Fail to create ResourceKey : (\"{regSpace}\", \"{regName}\", \"{idSpace}\", \"{idName}\") is invalid for a ResourceKey");
            }
            return key;
        }


        /// <summary>
        /// 尝试由两个 StringID 创建一个 StringKey
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="location"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool TryCreate(ResourceLocation registry, ResourceLocation location, out ResourceKey key)
        {
            if(!ResourceLocation.IsValidNamespace(registry.path))
            {
                key = Failed;
                return false;
            }
            key = DoCreate(registry, location, null);
            return true;
        }


        /// <summary>
        /// 尝试由四个字符串创建一个 StringKey
        /// </summary>
        /// <param name="regSpace"></param>
        /// <param name="regName"></param>
        /// <param name="idSpace"></param>
        /// <param name="idName"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool TryCreate(string regSpace, string regName, string idSpace, string idName, out ResourceKey key)
        {
            if(ResourceLocation.IsValidNamespace(regSpace) &&
                ResourceLocation.IsValidNamespace(regName) && 
                ResourceLocation.IsValidNamespace(idSpace) &&
                ResourceLocation.IsValidPath(idName))
            {
                key = DoCreate(regSpace, regName, idSpace, idName, null);
                return true;
            }
            key = Failed;
            return false;
        }


        /// <summary>
        /// 从 "{registry}/{location}" 形式的字符串解析并创建一个 StringKey，如果参数不符合要求则抛出异常
        /// </summary>
        /// <param name="keyString"></param>
        /// <returns></returns>
        public static ResourceKey Parse(string keyString)
        {
            if (!TryParse(keyString, out ResourceKey key))
            {
                throw new ArgumentException($"Fail to parse \"{keyString}\" as ResourceKey");
            }
            return key;
        }


        /// <summary>
        /// 解析并创建一个 StringKey，如果参数不符合要求则抛出异常，可指定起始位置和长度
        /// </summary>
        /// <param name="keyString"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static ResourceKey Parse(string keyString, int start, int length)
        {
            if (!TryParse(keyString, out ResourceKey key, start, length))
            {
                throw new ArgumentException($"Fail to parse \"{keyString}\" as ResourceKey");
            }
            return key;
        }


        /// <summary>
        /// 尝试解析并创建一个 StringKey
        /// </summary>
        /// <param name="keyString"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool TryParse(string keyString, out ResourceKey key)
        {
            return TryParse(keyString, out key, 0, keyString.Length);
        }


        /// <summary>
        /// 尝试解析并创建一个 StringKey，可指定起始位置和长度
        /// </summary>
        /// <param name="keyString"></param>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static bool TryParse(string keyString, out ResourceKey key, int start, int length)
        {
            if (!IsValidStringKey(keyString, out int i, out int j, out int k, start, length))
            {
                key = Failed;
                return false;
            }
            key = DoCreate(keyString[start..i], keyString[(i + 1)..j], keyString[(j + 1)..k], keyString[(k + 1)..(start + length)], keyString);
            return true;
        }


        /// <summary>
        /// 判断一个字符串是否是合法的 StringKey
        /// </summary>
        /// <param name="keyString"></param>
        /// <param name="i">keyString 中第一个 ':' 的下标</param>
        /// <param name="j">keyString 中 '/' 的下标</param>
        /// <param name="k">keyString 中第二个 ':' 的下标</param>
        /// <returns></returns>
        public static bool IsValidStringKey(string keyString, out int i, out int j, out int k)
        {
            return IsValidStringKey(keyString, out i, out j, out k, 0, keyString.Length);

        }


        /// <summary>
        /// 判断一个字符串是否是合法的 StringKey，可指定起始位置和长度
        /// </summary>
        /// <param name="keyString"></param>
        /// <param name="i">keyString 中第一个 ':' 的下标</param>
        /// <param name="j">keyString 中 '/' 的下标</param>
        /// <param name="k">keyString 中第二个 ':' 的下标</param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static bool IsValidStringKey(string keyString, out int i, out int j, out int k, int start, int length)
        {
            j = keyString.SimpleIndexOf(REGISTRY_SEPARATOR, start, length);
            if(j < 0 ||
                !ResourceLocation.IsValidStringID(keyString, out i, start, j - start) ||
                !ResourceLocation.IsValidStringID(keyString, out k, j + 1, start + length - j))
            {
                i = -1;
                k = -1;
                return false;
            }
            return true;
        }


        /// <summary>
        /// 执行实际的构造
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="id"></param>
        /// <param name="keyString"></param>
        /// <returns></returns>
        internal static ResourceKey DoCreate(ResourceLocation registry, ResourceLocation id, string? keyString)
        {
            if (keyString == null)
            {
                StringBuilder str = new StringBuilder(registry.namspace.Length + registry.path.Length + id.namspace.Length + id.path.Length + 3);
                str.Append(registry.namspace);
                str.Append(ResourceLocation.NAMESPACE_SEPARATOR);
                str.Append(registry.path);
                str.Append(REGISTRY_SEPARATOR);
                str.Append(id.namspace);
                str.Append(ResourceLocation.NAMESPACE_SEPARATOR);
                str.Append(id.path);
                keyString = str.ToString();
            }

            ResourceKey stringKey;
            // 这里加锁是为了保证不创建值相同的 ResourceKey 对象
            lock (RESOURCE_KEYS)
            {
                if (!RESOURCE_KEYS.TryGetValue(keyString, out stringKey))
                {
                    stringKey = new ResourceKey(registry, id, keyString);
                    RESOURCE_KEYS.TryAdd(keyString, stringKey);
                }
            }

            return stringKey;
        }


        /// <summary>
        /// 执行实际的构造
        /// </summary>
        /// <param name="regSpace"></param>
        /// <param name="regName"></param>
        /// <param name="idSpace"></param>
        /// <param name="idName"></param>
        /// <param name="keyString"></param>
        /// <returns></returns>
        internal static ResourceKey DoCreate(string regSpace, string regName, string idSpace, string idName, string? keyString)
        {
            return DoCreate(new ResourceLocation(regSpace, regName), new ResourceLocation(idSpace, idName), keyString);
        }


        private ResourceKey(ResourceLocation registry, ResourceLocation location, string keyString)
        {
            this.registry = registry;
            this.location = location;
            this.keyString = keyString;
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
            return keyString.GetHashCode();
        }


        /// <summary>
        /// 返回 keyString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return keyString;
        }
    }
}
