using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace StarCube.Utility
{
    /// <summary>
    /// 以字符串表示的 id ，既可以表示某个注册表、注册表中某个注册项，也可以表示磁盘上的某个文件
    /// </summary>
    /// 一个 StringID 含有两个字符串成员 : modid 和 path
    /// 它们各有所必须满足的格式
    /// StringID 用字符串表示为 $"{modid}:{name}"
    public sealed class StringID : IComparable<StringID>, IEquatable<StringID>
    {
        /// <summary>
        /// 构成一个合法 ResourceLocation 的字符串所需的最小长度
        /// </summary>
        public const int MIN_STRING_LENGTH = 3;


        public const char SEPARATOR_CHAR = ':';
        public const char PATH_SEPARATOR_CHAR = '/';


        /// <summary>
        /// StringID 池，所有 idString 及其对应的 StringID 都会放这里面
        /// </summary>
        private static readonly ConcurrentDictionary<string, StringID> stringToID = new ConcurrentDictionary<string, StringID>(StringComparer.Ordinal);


        /// <summary>
        /// 失败时会返回这个空的 StringID，注意这个 StringID 并不合法
        /// </summary>
        public static readonly StringID Failed = InternelCreate(string.Empty, -1);


        /// <summary>
        /// 创建一个 StringID，如果参数不符合要求则抛出异常
        /// </summary>
        /// <param name="modid"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static StringID Create(string modid, string name)
        {
            if (!IsValidModid(modid))
            {
                throw new ArgumentException($"Fail to create ResourceLocation : modid \"{modid}\" is invalid");
            }
            if (!IsValidName(name))
            {
                throw new ArgumentException($"Fail to create ResourceLocation : name \"{name}\" is invalid");
            }

            StringBuilder stringBuilder = StringUtil.StringBuilder;
            stringBuilder.Append(modid).Append(SEPARATOR_CHAR).Append(name);

            return InternelCreate(stringBuilder.ToString(), modid.Length, modid, name);
        }


        /// <summary>
        /// 创建一个 StringID，如果参数不符合要求则抛出异常
        /// </summary>
        /// <param name="modid"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static StringID Create(ReadOnlySpan<char> modid, ReadOnlySpan<char> name)
        {
            if (!IsValidModid(modid))
            {
                throw new ArgumentException($"Fail to create ResourceLocation : modid \"{modid.ToString()}\" is invalid");
            }
            if (!IsValidName(name))
            {
                throw new ArgumentException($"Fail to create ResourceLocation : name \"{name.ToString()}\" is invalid");
            }

            StringBuilder stringBuilder = StringUtil.StringBuilder;
            stringBuilder.Append(modid).Append(SEPARATOR_CHAR).Append(name);

            return InternelCreate(stringBuilder.ToString(), modid.Length);
        }


        /// <summary>
        /// 尝试创建一个 StringID
        /// </summary>
        /// <param name="modid"></param>
        /// <param name="name"></param>
        /// <returns>如果失败则返回 StringID.Failed</returns>
        public static bool TryCreate(string modid, string name, out StringID id)
        {
            if (!IsValidModid(modid) || !IsValidName(name))
            {
                id = Failed;
                return false;
            }

            StringBuilder stringBuilder = StringUtil.StringBuilder;
            stringBuilder.Append(modid).Append(SEPARATOR_CHAR).Append(name);

            id = InternelCreate(stringBuilder.ToString(), modid.Length, modid, name);
            return true;
        }


        /// <summary>
        /// 尝试创建一个 StringID
        /// </summary>
        /// <param name="modid"></param>
        /// <param name="name"></param>
        /// <returns>如果失败则返回 StringID.Failed</returns>
        public static bool TryCreate(ReadOnlySpan<char> modid, ReadOnlySpan<char> name, out StringID id)
        {
            if (!IsValidModid(modid) || !IsValidName(name))
            {
                id = Failed;
                return false;
            }

            StringBuilder stringBuilder = StringUtil.StringBuilder;
            stringBuilder.Append(modid).Append(SEPARATOR_CHAR).Append(name);

            id = InternelCreate(stringBuilder.ToString(), modid.Length);
            return true;
        }

        /// <summary>
        /// 尝试从 "{modid}:{name}" 形式的字符串解析并创建一个 StringID，如果参数不符合要求则抛出异常
        /// </summary>
        /// <param name="idString"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static StringID Parse(string idString)
        {
            if (TryParse(idString, out StringID id))
            {
                return id;
            }
            throw new ArgumentException($"Fail to parse StringID \"{idString}\"");
        }


        /// <summary>
        /// 解析并创建一个 StringID，如果参数不符合要求则抛出异常
        /// </summary>
        /// <param name="idString"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static StringID Parse(ReadOnlySpan<char> idString)
        {
            if (TryParse(idString, out StringID id))
            {
                return id;
            }
            throw new ArgumentException($"Fail to parse StringID \"{idString.ToString()}\"");
        }


        /// <summary>
        /// 尝试从 "{modid}:{name}" 形式的字符串解析并创建一个 StringID
        /// </summary>
        /// <param name="idString"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool TryParse(string idString, out StringID id)
        {
            if(IsValidID(idString, out int separatorIndex))
            {
                id = InternelCreate(idString, separatorIndex);
                return true;
            }
            id = Failed;
            return false;
        }


        /// <summary>
        /// 尝试从 "{modid}:{name}" 形式的字符串解析并创建一个 StringID
        /// </summary>
        /// <param name="idString"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool TryParse(ReadOnlySpan<char> idString, out StringID id)
        {
            if (IsValidID(idString, out int separatorIndex))
            {
                id = InternelCreate(idString.ToString(), separatorIndex);
                return true;
            }
            id = Failed;
            return false;
        }

        private static StringID InternelCreate(string idString, int separatorIndex, string? modid = null, string? name = null)
        {
            return stringToID.GetOrAdd(idString, DoCreate, (separatorIndex, modid, name));
        }

        private static StringID DoCreate(string idString, (int, string?, string?) tuple)
        {
            idString = string.Intern(idString);
            tuple.Item2 = tuple.Item2 == null ? null : string.Intern(tuple.Item2);
            tuple.Item3 = tuple.Item3 == null ? null : string.Intern(tuple.Item3);
            return new StringID(idString, tuple.Item1, tuple.Item2, tuple.Item3);
        }


        /// <summary>
        /// 判断一个 string id 是否有效，即非 null 且不为 Failed
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool Valid( [NotNullWhen(true)] StringID? id)
        {
            return id != null && id.idString.Length != 0;
        }


        /// <summary>
        /// 检查给定内容是否为合法的 modid
        /// </summary>
        /// <param name="modid"></param>
        /// <returns></returns>
        public static bool IsValidModid(ReadOnlySpan<char> modid)
        {
            if (modid.Length == 0 || modid[^1] == '-')
            {
                return false;
            }

            char firstChar = modid[0];
            if (!(char.IsLower(firstChar) || firstChar == '_'))
            {
                return false;
            }

            int lastMinus = -1;
            for (int i = 1; i < modid.Length; i++)
            {
                char c = modid[i];
                if (char.IsLower(c) || c == '_' || char.IsNumber(c))
                {
                    continue;
                }
                if (c == '-')
                {
                    if (lastMinus == i - 1)
                    {
                        return false;
                    }
                    lastMinus = i;
                    continue;
                }

                return false;
            }

            return true;
        }


        /// <summary>
        /// 检查给定内容是否为合法的 name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool IsValidName(ReadOnlySpan<char> name)
        {
            if (name.Length == 0 || name[^1] == PATH_SEPARATOR_CHAR || name[^1] == '-')
            {
                return false;
            }

            int start = 0;
            while(start < name.Length)
            {
                int len = 0;
                int i;
                for (i = start; i < name.Length; ++i)
                {
                    if (name[i] == PATH_SEPARATOR_CHAR)
                    {
                        len = i - start;
                        break;
                    }
                }
                if(i >= name.Length)
                {
                    len = name.Length - start;
                }

                ReadOnlySpan<char> slice = name.Slice(start, len);
                if (!IsValidModid(slice))
                {
                    return false;
                }

                start = start + len + 1;
                if(start > name.Length)
                {
                    break;
                }
            }

            return true;
        }


        /// <summary>
        /// 检查给定内容是否为合法的 StringID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsValidID(ReadOnlySpan<char> id)
        {
            int separatorIndex = id.IndexOf(SEPARATOR_CHAR);

            if (separatorIndex < 0)
            {
                return false;
            }

            return IsValidModid(id[..separatorIndex]) && IsValidName(id[(separatorIndex + 1)..]);
        }


        /// <summary>
        /// 检查给定内容是否为合法的 StringID，并返回分隔符的下标
        /// </summary>
        /// <param name="id"></param>
        /// <param name="separatorIndex"></param>
        /// <returns></returns>
        public static bool IsValidID(ReadOnlySpan<char> id, out int separatorIndex)
        {
            separatorIndex = id.IndexOf(SEPARATOR_CHAR);

            if (separatorIndex < 0)
            {
                return false;
            }

            return IsValidModid(id[..separatorIndex]) && IsValidName(id[(separatorIndex + 1)..]);
        }



        /* 以下两个属性最多只在第一次调用才会创建 string 对象，且创建后会自动 intern */

        /// <summary>
        /// 此 StringID 的 modid 的 string
        /// </summary>
        public string ModidString
        {
            get
            {
                cachedModidString ??= string.Intern(Modid.ToString());
                return cachedModidString;
            }
        }


        /// <summary>
        /// 此 StringID 的 name 的 string
        /// </summary>
        public string NameString
        {
            get
            {
                cachedNameString ??= string.Intern(Name.ToString());
                return cachedNameString;
            }
        }


        /// <summary>
        /// StringID 的 modid
        /// </summary>
        public ReadOnlySpan<char> Modid => idString.AsSpan(0, separatorIndex);


        /// <summary>
        /// StringID 的 name
        /// </summary>
        public ReadOnlySpan<char> Name => idString.AsSpan(separatorIndex + 1);


        /// <summary>
        /// id string 的长度
        /// </summary>
        public int Length => idString.Length;


        /// <summary>
        /// 比较两个 StringID，这个方法先比较 name 再比较 modid
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(StringID? other)
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }
            if (other == null)
            {
                return -1;
            }
            if (ReferenceEquals(idString, other.idString))
            {
                return 0;
            }

            int rns = Name.CompareTo(other.Name, StringComparison.Ordinal);
            if (rns != 0)
            {
                return rns;
            }
            return Modid.CompareTo(other.Modid, StringComparison.Ordinal);
        }


        /// <summary>
        /// 比较两个 StringID，这个方法先比较 modid 再比较 name
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareModidFirst(StringID? other)
        {
            if (this == other)
            {
                return 0;
            }
            if (other == null)
            {
                return -1;
            }
            return string.Compare(idString, other.idString, StringComparison.Ordinal);
        }


        /// <summary>
        /// 返回 id string 的 HashCode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return idString.GetHashCode();
        }


        /// <summary>
        /// 返回 id string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return idString;
        }


        /// <summary>
        /// 比较两个 StringID 是否相等，这会比较两者的字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            return obj is StringID other && Equals(other);
        }


        /// <summary>
        /// 比较两个 StringID 是否相等，这会比较两者的字符串
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(StringID other)
        {
            return idString.Equals(other.idString, StringComparison.Ordinal);
        }


        private StringID(string idString, int separatorIndex, string? cachedModidString, string? cachedNameString)
        {
            this.idString = idString;
            this.separatorIndex = separatorIndex;
            this.cachedModidString = cachedModidString;
            this.cachedNameString = cachedNameString;
        }

        public readonly string idString;
        public readonly int separatorIndex;

        private string? cachedModidString = null;
        private string? cachedNameString = null;
    }
}
