using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json.Linq;

using StarCube.Resource;
using StarCube.Utility;

namespace StarCube.Core.Tag
{
    /// <summary>
    /// 表示一个 Tag 定义文件的数据结构
    /// </summary>
    public class TagData
    {
        public const string OVERRIDE_STRING = "override";
        public const string ENTRIES_STRING = "entries";

        /// <summary>
        /// TagData 中 entries 数组中一个 entry，可用一个字符串表示
        /// </summary>
        /// 一个 entry 的字符串合法格式为 [-][#]{id}[?]
        /// id 是一个合法的 ResourceLocation
        /// - 代表此项需要被移除
        /// # 代表此项是个 Tag
        /// ? 代表对此项的依赖是非必须的
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

            public readonly ResourceLocation id;

            public readonly EntryType entryType;

            public readonly bool optional;

            public readonly bool toRemove;

            public Entry(ResourceLocation id, EntryType entryType, bool optional, bool toRemove)
            {
                this.id = id;
                this.entryType = entryType;
                this.optional = optional;
                this.toRemove = toRemove;
            }

            public override string ToString()
            {
                StringBuilder str = new StringBuilder();
                if (toRemove)
                {
                    str.Append(REMOVE_CHAR);
                }
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

            /// <summary>
            /// 尝试从字符串中解析出 Entry
            /// </summary>
            /// <param name="entryString"></param>
            /// <returns></returns>
            public static bool TryParse(string entryString, out Entry entry)
            {
                int idStart = 0;
                int idLength = entryString.Length;
                ResourceLocation? id;
                EntryType entryType = EntryType.Element;
                bool optional = false;
                bool toRemove = false;
                // 一个 entryString 的长度至少是一个合法 ResourceLocation 的长度
                if (entryString.Length < ResourceLocation.MIN_STRING_LENGTH)
                {
                    entry = new Entry();
                    return false;
                }
                if (entryString.StartsWith(REMOVE_CHAR))
                {
                    toRemove = true;
                    ++idStart;
                    --idLength;
                }
                if (entryString.EndsWith(OPTIONAL_CHAR))
                {
                    optional = true;
                    --idLength;
                }
                // 注意: 这里不需要判断长度是因为 entryString 至少有 3 字符长，如果后续扩展功能则可能需要再次判断长度
                if (!toRemove && entryString.StartsWith(TAG_TYPE_CHAR) || toRemove && entryString[1] == TAG_TYPE_CHAR)
                {
                    entryType = EntryType.Tag;
                    ++idStart;
                    --idLength;
                }
                id = ResourceLocation.TryParse(entryString.Substring(idStart, idLength));
                if (id == null)
                {
                    entry = new Entry();
                    return false;
                }
                entry = new Entry(id, entryType, optional, toRemove);
                return true;
            }
        }

        /// <summary>
        /// 清除之前的所有 entry
        /// </summary>
        public readonly bool isOverride;

        /// <summary>
        /// 本 TagData 所添加的 entry
        /// </summary>
        public readonly Entry[] entries;

        public TagData(bool isOverride, Entry[] entries)
        {
            this.isOverride = isOverride;
            this.entries = entries;
        }

        /// <summary>
        /// 将一个 TagData 结构转化为 JsonObject
        /// </summary>
        /// <param name="tagData"></param>
        /// <returns></returns>
        public JObject ToJson()
        {
            JObject json = new JObject();
            if (isOverride)
            {
                json.Add(OVERRIDE_STRING, new JValue(isOverride));
            }
            JArray entryArray = new JArray();
            foreach(Entry entry in entries)
            {
                entryArray.Add(new JValue(entry.ToString()));
            }
            json.Add(ENTRIES_STRING, entryArray);
            return json;
        }

        /// <summary>
        /// 从一个 JsonObject 中构造 TagData
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        /// 此方法只关心自己需要的数据，不会严格检查格式，所以通过这种方式构造的 TagData 再转化为 Json，结果与原始 Json 不一定相同
        public static TagData? FromJson(JObject json)
        {
            List<Entry> entries = new List<Entry>();
            if (!JsonHelper.TryGetBoolean(json, OVERRIDE_STRING, out bool isOverride))
            {
                isOverride = false;
            }
            if(!JsonHelper.TryGetArray(json, ENTRIES_STRING, out JArray? entryArray))
            {
                return null;
            }
            foreach(JToken token in entryArray)
            {
                if(token is JValue jValue && jValue.Value is string entryString && Entry.TryParse(entryString, out Entry entry))
                {
                    entries.Add(entry);
                }
                else
                {
                    return null;
                }
            }
            return new TagData(isOverride, entries.ToArray());
        }
    }
}
