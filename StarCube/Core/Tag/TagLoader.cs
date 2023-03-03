using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using StarCube.Data;
using StarCube.Data.Loading;
using StarCube.Data.DependencyResolver;
using StarCube.Core.Registry;
using StarCube.Game.Block;

namespace StarCube.Core.Tag
{
    public class TagLoader : IDataLoader
    {
        public void Run(IDataProvider dataProvider)
        {
            LoadTagData(dataProvider, out List<TagData> loadedTagData);
            BuildTags(loadedTagData, out List<Tag<Block>> tags);
            TagManager<Block> tagManager = new TagManager<Block>(tags);
        }

        /// <summary>
        /// 以单线程同步方式加载并解析 Tag 数据为 TagData
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="loadedTagData"></param>
        private void LoadTagData(IDataProvider dataProvider, out List<TagData> loadedTagData)
        {
            Dictionary<StringID, TagData.Builder> tagBuilders = new Dictionary<StringID, TagData.Builder>();
            foreach (IDataProvider.DataEntry entry in dataProvider.EnumerateData(Tag.DataRegistry))
            {
                JObject json = JObject.Load(new JsonTextReader(new StreamReader(entry.dataStream)));
                if (!tagBuilders.TryGetValue(entry.id, out TagData.Builder builder))
                {
                    builder = new TagData.Builder(entry.id);
                    tagBuilders.Add(entry.id, builder);
                }
                builder.AddFromJson(json);
            }

            loadedTagData = new List<TagData>();
            foreach (KeyValuePair<StringID, TagData.Builder> pair in tagBuilders)
            {
                loadedTagData.Add(pair.Value.Build());
            }
        }

        private void BuildTags(List<TagData> unresolvedTagData, out List<Tag<Block>> tags)
        {
            Func<StringID, Block?> blockGetter = (id) => Registries.BlockRegistry.TryGet(id, out Block? block) ? block : null;
            TagBuilder<Block> blockTagBuilder = new TagBuilder<Block>(blockGetter);
            DependencyDataResolver<StringID, TagData, Tag<Block>> dataResolver =
                new DependencyDataResolver<StringID, TagData, Tag<Block>>(unresolvedTagData, blockTagBuilder);
            if(dataResolver.BuildResolvedData(out Dictionary<StringID, Tag<Block>>? resolvedData, false))
            {
                tags = resolvedData.Values.ToList();
            }
            else
            {
                throw new Exception();
            }
        }

        public string Name => "tagloader [builtin]";
    }
}
