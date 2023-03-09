using System.Collections.Generic;
using System.Text;
using System.Diagnostics.CodeAnalysis;

using Newtonsoft.Json.Linq;

using StarCube.Utility;
using StarCube.Data;
using StarCube.Data.Loading;
using StarCube.Data.DependencyResolver;
using StarCube.Data.Exception;

namespace StarCube.Core.Tag.Data
{
    /// <summary>
    /// 表示一个 Tag 定义文件的数据结构
    /// </summary>
    public class TagData : IUnresolvedData<TagData>
    {
        public const string OVERRIDE_STRING = "override";
        public const string ENTRIES_STRING = "entries";

        public static readonly StringID DataRegistry = StringID.Create(Constants.DEFAULT_NAMESPACE, "tag");

        public static readonly IDataReader<TagData> DataReader = new DataReaderWrapper<TagData, JObject>(RawDataReaders.JSON, TryParse);

        public static bool TryParse(JObject json, StringID id, [NotNullWhen(true)] out TagData? data)
        {
            Builder builder = new Builder(id);
            builder.AddFromJson(json);

            data = builder.Build();

            return true;
        }

        /// <summary>
        /// TagData 中 entries 数组中一个 entry，可用一个字符串表示
        /// </summary>
        /// 一个 entry 的字符串合法格式为 [-][#]{id}[?]
        /// id 是一个合法的 ResourceLocation
        /// - 代表此项需要被移除，注意如果此项对应的 Entry 不存在或者类型不对，则直接忽略
        /// # 代表此项是个 Tag
        /// ? 代表对此项的依赖是非必需的
        /// 三者可以任意组合使用
        /// - 与 ? 同时使用时，? 会被忽略
        public readonly struct Entry
        {
            public const char TAG_TYPE_CHAR = '#';
            public const char OPTIONAL_CHAR = '?';
            public const char REMOVE_CHAR = '-';

            public enum EntryType
            {
                Element,
                Tag,
            }

            public readonly StringID id;

            public readonly EntryType entryType;

            public readonly bool optional;

            public Entry(StringID id, EntryType entryType, bool optional)
            {
                this.id = id;
                this.entryType = entryType;
                this.optional = optional;
            }

            /// <summary>
            /// 返回一个 [-][#]{id}[?] 格式的字符串
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                StringBuilder str = new StringBuilder();
                if (entryType == EntryType.Tag)
                {
                    str.Append(TAG_TYPE_CHAR);
                }
                str.Append(id.ToString());
                if (optional)
                {
                    str.Append(OPTIONAL_CHAR);
                }
                return str.ToString();
            }
        }

        /// <summary>
        /// 按顺序读入多个 JsonObject 并构建 TagData
        /// </summary>
        public class Builder
        {
            private readonly StringID id;

            private readonly List<Entry> entries = new List<Entry>();

            public static Builder Create(StringID id)
            {
                return new Builder(id);
            }

            public Builder(StringID id)
            {
                this.id = id;
            }

            public Builder AddFromJson(JObject json)
            {
                // override 属性会使所有已存在的 entry 被清除
                if (json.TryGetBoolean(OVERRIDE_STRING, out bool isOverride) && isOverride)
                {
                    entries.Clear();
                }

                // 读取 entries
                if (json.TryGetArray(ENTRIES_STRING, out JArray? array))
                {
                    foreach (JToken token in array)
                    {
                        if (!token.TryConvertToString(out string entryString))
                        {
                            throw new DataParseException($"an entry in \"{ENTRIES_STRING}\" array is not string");
                        }

                        if (!TryParse(entryString, out StringID entryId, out Entry.EntryType entryType, out bool optional, out bool toRemove))
                        {
                            throw new DataParseException($"failed to parse \"{entryString}\") as StringID");
                        }

                        if (toRemove)
                        {
                            Remove(entryId, entryType);
                        }
                        else
                        {
                            entries.Add(new Entry(id, entryType, optional));
                        }
                    }
                }

                return this;
            }

            public TagData Build()
            {
                return new TagData(id, entries);
            }

            private void Remove(StringID entryId, Entry.EntryType entryType)
            {
                for (int i = 0; i < entries.Count; ++i)
                {
                    if (entries[i].entryType == entryType && entries[i].id == entryId)
                    {
                        entries.RemoveAt(i);
                        break;
                    }
                }
            }

            private static bool TryParse(string entryString, out StringID id, out Entry.EntryType type, out bool optional, out bool toRemove)
            {
                int idStart = 0;
                int idLength = entryString.Length;
                type = Entry.EntryType.Element;
                optional = false;
                toRemove = false;

                // 一个 entryString 的长度至少是一个合法 ResourceLocation 的长度
                if (entryString.Length < StringID.MIN_STRING_LENGTH)
                {
                    id = StringID.Failed;
                    return false;
                }

                if (entryString.StartsWith(Entry.REMOVE_CHAR))
                {
                    toRemove = true;
                    ++idStart;
                    --idLength;
                }
                if (entryString.EndsWith(Entry.OPTIONAL_CHAR))
                {
                    optional = true;
                    --idLength;
                }
                // 注意: 这里不需要判断长度是因为 entryString 至少有 3 字符长，如果后续扩展功能则可能需要再次判断长度
                if (!toRemove && entryString.StartsWith(Entry.TAG_TYPE_CHAR) || toRemove && entryString[1] == Entry.TAG_TYPE_CHAR)
                {
                    type = Entry.EntryType.Tag;
                    ++idStart;
                    --idLength;
                }

                if (!StringID.TryParse(entryString, out id, idStart, idLength))
                {
                    return false;
                }

                return true;
            }
        }


        public StringID ID => id;

        public TagData UnresolvedData => this;

        public IEnumerable<StringID> RequiredDependencies => requiredDependencies;

        public IEnumerable<StringID> OptionalDependencies => optionalDependencies;


        private readonly List<StringID> requiredDependencies;

        private readonly List<StringID> optionalDependencies;

        public readonly StringID id;

        /// <summary>
        /// 本 TagData 所添加的 entry
        /// </summary>
        public readonly List<Entry> entries;

        public TagData(StringID id, List<Entry> entries)
        {
            this.id = id;
            this.entries = entries;

            optionalDependencies = new List<StringID>();
            requiredDependencies = new List<StringID>();
            for (int i = 0; i < entries.Count; ++i)
            {
                Entry entry = entries[i];
                if (entry.entryType == Entry.EntryType.Tag)
                {
                    if (entry.optional)
                    {
                        optionalDependencies.Add(entry.id);
                    }
                    else
                    {
                        requiredDependencies.Add(entry.id);
                    }
                }
            }
        }
    }
}
