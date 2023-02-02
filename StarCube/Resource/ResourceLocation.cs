using System;
using System.Text.RegularExpressions;

namespace StarCube.Resource
{
    /// <summary>
    /// 表示一个资源的路径，既可以表示某个注册表、注册表中某个注册项，也可以表示磁盘上的某个文件
    /// </summary>
    /// 一个 ResourceLocation 含有两个字符串成员：
    /// namspace 和 path，它们各有所必须满足的格式
    /// ResourceLocation 用字符串表示为 "{namspace}:{path}"
    public sealed class ResourceLocation : IComparable<ResourceLocation>, IEquatable<ResourceLocation>
    {
        /// <summary>
        /// 构成一个合法 ResourceLocation 的字符串所需的最小长度
        /// </summary>
        public const int MIN_STRING_LENGTH = 3;

        public const char NAMESPACE_SEPARATOR = ':';
        public const string NAMESPACE_PATTERN = "[a-z0-9_]+(-[a-z0-9_]+)*";
        public const string PATH_PATTERN = "[a-z0-9_]+(-[a-z0-9_]+)*(/[a-z0-9_]+(-[a-z0-9_]+)*)*";

        /// <summary>
        /// 占位符，相当于空
        /// </summary>
        public static ResourceLocation Default = new ResourceLocation("_", "_");

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
        public static ResourceLocation Create(string namspace, string path)
        {
            if (!IsValidNamespace(namspace))
            {
                throw new ArgumentException($"Fail to create ResourceLocation : namespace \"{namspace}\" is invalid");
            }
            if (!IsValidPath(path))
            {
                throw new ArgumentException($"Fail to create ResourceLocation : path \"{path}\" is invalid");
            }
            return new ResourceLocation(namspace, path);
        }

        /// <summary>
        /// 创建一个 ResourceLocation，如果参数不符合要求则返回 null
        /// </summary>
        /// <param name="namspace"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static ResourceLocation? TryCreate(string namspace, string path)
        {
            if (!IsValidNamespace(namspace))
            {
                return null;
            }
            if (!IsValidPath(path))
            {
                return null;
            }
            return new ResourceLocation(namspace, path);
        }

        /// <summary>
        /// 尝试从 "{namspace}:{path}" 形式的字符串创建一个 ResourceLocation，如果参数不符合要求则抛出异常
        /// </summary>
        /// <param name="locationString"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static ResourceLocation Parse(string locationString)
        {
            ResourceLocation? loc = TryParse(locationString);
            if (loc == null)
            {
                throw new ArgumentException($"Fail to parse ResourceLocation \"{locationString}\"");
            }
            return loc;
        }

        /// <summary>
        /// 尝试从 "{namspace}:{path}" 形式的字符串创建一个 ResourceLocation，如果参数不符合要求则返回 null
        /// </summary>
        /// <param name="locationString"></param>
        /// <returns></returns>
        public static ResourceLocation? TryParse(string locationString)
        {
            string[] splits = locationString.Split(NAMESPACE_SEPARATOR);
            if (splits.Length != 2)
            {
                return null;
            }
            return TryCreate(splits[0], splits[1]);
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
        /// 检查某个 path 是否符合格式要求
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsValidPath(string path)
        {
            Match match = PathRegex.Match(path);
            return match.Success && match.Index == 0 && match.Length == path.Length;
        }

        private ResourceLocation(string namspace, string path)
        {
            this.namspace = namspace;
            this.path = path;
        }

        /// <summary>
        /// 比较两个 ResourceLocation，这个方法先比较 path 再比较 namspace
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(ResourceLocation? other)
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
        public int CompareNamespaceFirst(ResourceLocation? other)
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
            return obj is ResourceLocation other && Equals(other);
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
            return namspace + NAMESPACE_SEPARATOR + path;
        }

        public bool Equals(ResourceLocation other)
        {
            if (object.ReferenceEquals(this, other))
            {
                return true;
            }
            return path.Equals(other.path, StringComparison.Ordinal) && namspace.Equals(other.namspace, StringComparison.Ordinal);
        }
    }
}
