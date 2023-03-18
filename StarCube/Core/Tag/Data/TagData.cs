using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

using Newtonsoft.Json.Linq;

using StarCube.Utility;
using StarCube.Data.Loading;
using StarCube.Data.DependencyResolver;

namespace StarCube.Core.Tag.Data
{
    public readonly struct TagDataEntry
    {
        public const char TAG_TYPE_CHAR = '#';
        public const char OPTIONAL_CHAR = '?';
        public const char REMOVE_CHAR = '-';

        public enum EntryType
        {
            Element,
            Tag,
        }


        public static bool TryParse(string entryString, out TagDataEntry entry)
        {
            entry = new TagDataEntry();

            int idStart = 0;
            int idLength = entryString.Length;
            EntryType type = EntryType.Element;
            bool optional = false;
            bool remove = false;

            // 一个 entryString 的长度至少是一个合法 StringID 的长度
            if (entryString.Length < StringID.MIN_STRING_LENGTH)
            {
                return false;
            }

            if (entryString.StartsWith(REMOVE_CHAR))
            {
                remove = true;
                ++idStart;
                --idLength;
            }
            if (entryString.EndsWith(OPTIONAL_CHAR))
            {
                optional = true;
                --idLength;
            }
            // 注意: 这里不需要判断长度是因为 entryString 至少有 3 字符长，如果后续扩展功能则可能需要再次判断长度
            if (!remove && entryString.StartsWith(TAG_TYPE_CHAR) || remove && entryString[1] == TAG_TYPE_CHAR)
            {
                type = EntryType.Tag;
                ++idStart;
                --idLength;
            }

            if (!StringID.TryParse(entryString.AsSpan(idStart, idLength), out StringID id))
            {
                return false;
            }

            entry = new TagDataEntry(id, type, optional, remove);
            return true;
        }


        /// <summary>
        /// 返回一个 [-][#]{id}[?] 格式的字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            int len = 0;
            len += remove ? 1 : 0;
            len += type == EntryType.Tag ? 1 : 0;
            len += id.Length;
            len += optional ? 1 : 0;
            StringBuilder str = new StringBuilder(len);

            if (remove)
            {
                str.Append(REMOVE_CHAR);
            }
            if (type == EntryType.Tag)
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

        public readonly StringID id;

        public readonly EntryType type;

        public readonly bool optional;

        public readonly bool remove;

        public TagDataEntry(StringID id, EntryType type, bool optional, bool remove)
        {
            this.id = id;
            this.type = type;
            this.optional = optional;
            this.remove = remove;
        }
    }

    /// <summary>
    /// 表示一个 Tag 定义文件的数据结构
    /// </summary>

    public class TagData : IUnresolvedData<TagData>
    {
        public static readonly StringID DataRegistry = StringID.Create(Constants.DEFAULT_NAMESPACE, "tag");

        public static readonly IDataReader<TagData> DataReader = new DataReaderWrapper<TagData, JObject>(RawDataReaders.JSON, TryParseFromJson);


        public static bool TryParseFromJson(JObject json, StringID id, [NotNullWhen(true)] out TagData? data)
        {
            data = null;

            if (!json.TryGetBoolean("override", out bool clear))
            {
                clear = false;
            }

            List<TagDataEntry> entries = new List<TagDataEntry>();
            if (json.TryGetArray("entries", out JArray? entryArray))
            {
                foreach (JToken token in entryArray)
                {
                    if(!(token is JValue jValue &&
                        jValue.Value is string entryString &&
                        TagDataEntry.TryParse(entryString, out TagDataEntry entry)))
                    {
                        return false;
                    }

                    entries.Add(entry);
                }
            }

            data = new TagData(id, clear, entries);
            return true;
        }


        public static TagData Combine(StringID id, IEnumerable<TagData> tagDataList)
        {
            List<TagDataEntry> entries = new List<TagDataEntry>();
            foreach (TagData tagData in tagDataList)
            {
                if(tagData.clear)
                {
                    entries.Clear();
                }
                foreach (TagDataEntry entry in tagData.entries)
                {
                    if(entry.remove)
                    {
                        int i;
                        for (i = 0; i < entries.Count; i++)
                        {
                            if(entries[i].id.Equals(entry.id) && entries[i].type == entry.type)
                            {
                                entries.RemoveAt(i);
                                break;
                            }
                        }
                    }
                    else
                    {
                        entries.Add(entry);
                    }
                }
            }

            return new TagData(id, false, entries);
        }


        StringID IStringID.ID => id;

        public TagData UnresolvedData => this;
        public IEnumerable<StringID> RequiredDependencies => requiredDependencies;
        public IEnumerable<StringID> OptionalDependencies => optionalDependencies;


        public TagData(StringID id, bool clear, List<TagDataEntry> entries)
        {
            this.id = id;
            this.entries = entries;
            this.clear = clear;

            optionalDependencies = new List<StringID>();
            requiredDependencies = new List<StringID>();
            for (int i = 0; i < entries.Count; ++i)
            {
                TagDataEntry entry = entries[i];
                if (entry.type == TagDataEntry.EntryType.Tag)
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

        public readonly StringID id;

        public readonly bool clear;

        public readonly List<TagDataEntry> entries;

        private readonly List<StringID> requiredDependencies;
        private readonly List<StringID> optionalDependencies;
    }
}
