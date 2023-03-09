using System;
using System.Text.RegularExpressions;

namespace StarCube.Utility
{
    /// <summary>
    /// 表示一个资源的路径，既可以表示某个注册表、注册表中某个注册项，也可以表示磁盘上的某个文件
    /// </summary>
    /// 一个 ResourceLocation 含有两个字符串成员 : namspace 和 path
    /// 它们各有所必须满足的格式
    /// ResourceLocation 用字符串表示为 $"{namspace}:{path}"
    public sealed class StringID : IComparable<StringID>, IEquatable<StringID>
    {
        /// <summary>
        /// 构成一个合法 ResourceLocation 的字符串所需的最小长度
        /// </summary>
        public const int MIN_STRING_LENGTH = 3;


        public const char SEPARATOR_CHAR = ':';
        public const char PATH_SEPARATOR_CHAR = '/';

        public const string NAMESPACE_PATTERN = "[a-z0-9_]+(-[a-z0-9_]+)*";
        public const string PATH_PATTERN = "[a-z0-9_]+(-[a-z0-9_]+)*(/[a-z0-9_]+(-[a-z0-9_]+)*)*";


        /// <summary>
        /// 占位符，解析或创建失败时会返回这个 ResourceLocation
        /// </summary>
        public static readonly StringID Failed = new StringID("_", "_");


        public readonly string namspace;
        public readonly string path;

        private static readonly Regex NamespaceRegex = new Regex(NAMESPACE_PATTERN, RegexOptions.Compiled);
        private static readonly Regex PathRegex = new Regex(PATH_PATTERN, RegexOptions.Compiled);


        /// <summary>
        /// 创建一个 ResourceLocation，如果参数不符合要求则抛出异常
        /// </summary>
        /// <param name="namspace"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static StringID Create(string namspace, string path)
        {
            if (!IsValidNamespace(namspace))
            {
                throw new ArgumentException($"Fail to create ResourceLocation : namespace \"{namspace}\" is invalid");
            }
            if (!IsValidPath(path))
            {
                throw new ArgumentException($"Fail to create ResourceLocation : path \"{path}\" is invalid");
            }
            return new StringID(namspace, path);
        }


        /// <summary>
        /// 尝试创建一个 ResourceLocation
        /// </summary>
        /// <param name="namspace"></param>
        /// <param name="path"></param>
        /// <returns>如果失败则返回 ResourceLocation.Failed</returns>
        public static bool TryCreate(string namspace, string path, out StringID location)
        {
            if (!IsValidNamespace(namspace) || !IsValidPath(path))
            {
                location = Failed;
                return false;
            }
            location = new StringID(namspace, path);
            return true;
        }


        /// <summary>
        /// 尝试从 "{namspace}:{path}" 形式的字符串解析并创建一个 ResourceLocation，如果参数不符合要求则抛出异常
        /// </summary>
        /// <param name="locationString"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static StringID Parse(string locationString)
        {
            if (TryParse(locationString, out StringID location))
            {
                return location;
            }
            throw new ArgumentException($"Fail to parse ResourceLocation \"{locationString}\"");
        }


        /// <summary>
        /// 解析并创建一个 ResourceLocation，如果参数不符合要求则抛出异常，可指定起始位置和长度
        /// </summary>
        /// <param name="locationString"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static StringID Parse(string locationString, int start, int length)
        {
            if (TryParse(locationString, out StringID location, start, length))
            {
                return location;
            }
            throw new ArgumentException($"Fail to parse ResourceLocation \"{locationString[start..(length + start)]}\"");
        }


        /// <summary>
        /// 尝试从 "{namspace}:{path}" 形式的字符串解析并创建一个 ResourceLocation
        /// </summary>
        /// <param name="locationString"></param>
        /// <returns>如果失败则返回 ResourceLocation.Failed</returns>
        public static bool TryParse(string locationString, out StringID location)
        {
            return TryParse(locationString, out location, 0, locationString.Length);
        }


        /// <summary>
        /// 尝试解析并创建一个 ResourceLocation，可指定起始位置和长度
        /// </summary>
        /// <param name="locationString"></param>
        /// <param name="location"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns>如果失败则返回 ResourceLocation.Failed</returns>
        public static bool TryParse(string locationString, out StringID location, int start, int length)
        {
            if (!IsValidStringID(locationString, out int i, start, length))
            {
                location = Failed;
                return false;
            }
            location = Create(locationString[start..i], locationString[(i + 1)..(start + length)]);
            return true;
        }


        /// <summary>
        /// 判断一个字符串是否是合法的 StringID
        /// </summary>
        /// <param name="idString"></param>
        /// <param name="i">分隔符在 idString 中的下标</param>
        /// <returns></returns>
        public static bool IsValidStringID(string idString, out int i)
        {
            return IsValidStringID(idString, out i, 0, idString.Length);
        }


        /// <summary>
        /// 判断一个字符串是否是合法的 StringID，可指定起始位置和长度
        /// </summary>
        /// <param name="idString"></param>
        /// <param name="i">分隔符在 idString 中的下标</param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static bool IsValidStringID(string idString, out int i, int start, int length)
        {
            i = idString.SimpleIndexOf(SEPARATOR_CHAR, start, length);
            if (i < 0 ||
                !IsValidNamespace(idString, start, i - start) ||
                !IsValidPath(idString, i + 1, length + start - i - 1))
            {
                return false;
            }
            return true;
        }


        /// <summary>
        /// 检查某个 namespace 是否符合格式要求
        /// </summary>
        /// <param name="namspace"></param>
        /// <returns></returns>
        public static bool IsValidNamespace(string namspace)
        {
            Match match = NamespaceRegex.Match(namspace);
            return match.Success && match.Index == 0 && match.Length == namspace.Length;
        }


        /// <summary>
        /// 检查某个 namespace 是否符合格式要求，可指定起始位置和长度
        /// </summary>
        /// <param name="namspace"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static bool IsValidNamespace(string namspace, int start, int length)
        {
            Match match = NamespaceRegex.Match(namspace, start);
            return match.Success && match.Index == start && match.Length == length;
        }


        /// <summary>
        /// 检查某个 path 是否符合格式要求
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsValidPath(string path)
        {
            Match match = PathRegex.Match(path);
            return match.Success && match.Index == 0 && match.Length == path.Length;
        }


        /// <summary>
        /// 检查某个 path 是否符合格式要求，可指定起始位置和长度
        /// </summary>
        /// <param name="path"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static bool IsValidPath(string path, int start, int length)
        {
            Match match = PathRegex.Match(path, start);
            return match.Success && match.Index == start && match.Length == length;
        }


        internal StringID(string namspace, string path)
        {
            this.namspace = namspace;
            this.path = path;
        }


        /// <summary>
        /// 比较两个 ResourceLocation，这个方法先比较 path 再比较 namspace
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(StringID? other)
        {
            if (this == other)
            {
                return 0;
            }
            if (other == null)
            {
                return -1;
            }
            int rns = path.CompareTo(other?.path);
            if (rns != 0)
            {
                return rns;
            }
            return namspace.CompareTo(other?.namspace);
        }


        /// <summary>
        /// 比较两个 ResourceLocation，这个方法先比较 namspace 再比较 path
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareNamespaceFirst(StringID? other)
        {
            if (this == other)
            {
                return 0;
            }
            if (other == null)
            {
                return -1;
            }
            int rns = namspace.CompareTo(other?.namspace);
            if (rns != 0)
            {
                return rns;
            }
            return path.CompareTo(other?.path);
        }


        public override int GetHashCode()
        {
            return 31 * namspace.GetHashCode() + path.GetHashCode();
        }


        /// <summary>
        /// 返回一个 "{namspace}:{path}" 形式的字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return namspace + SEPARATOR_CHAR + path;
        }


        /// <summary>
        /// 比较两个 ResourceLocation 是否相等，这会比较两者的两个字符串成员
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            if (obj == null)
            {
                return false;
            }
            return obj is StringID other && Equals(other);
        }


        /// <summary>
        /// 比较两个 ResourceLocation 是否相等，这会比较两者的两个字符串成员
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(StringID other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return path.Equals(other.path, StringComparison.Ordinal) && namspace.Equals(other.namspace, StringComparison.Ordinal);
        }
    }
}
