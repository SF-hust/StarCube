using System;
using System.Collections.Generic;
using System.Linq;

using StarCube.Data;
using StarCube.Data.Loading;
using StarCube.Data.Provider;
using StarCube.Data.DependencyResolver;

using static StarCube.Data.Provider.IDataProvider;

namespace StarCube.Core.Tag.Data
{
    public class TagLoader<T> : IDataLoader
        where T : class, IStringID
    {
        public void Run(IDataProvider dataProvider)
        {
            LoadTagData(dataProvider, out List<TagData> allTagData);
            BuildTags(allTagData, out List<Tag<T>> tags);
            TagManager<T> tagManager = new TagManager<T>(tags);
            consumeTagManager(tagManager);
        }

        /// <summary>
        /// 加载并解析 Tag 数据为 TagData
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="loadedTagData"></param>
        private void LoadTagData(IDataProvider dataProvider, out List<TagData> loadedTagData)
        {
            loadedTagData = dataProvider.EnumerateData<TagData>(TagData.DataRegistry, tagHolderType + "/", TagData.DataReader);
        }

        /// <summary>
        /// 解析并构造 Tag
        /// </summary>
        /// <param name="unresolvedTagData"></param>
        /// <param name="tags"></param>
        /// <exception cref="Exception"></exception>
        private void BuildTags(List<TagData> unresolvedTagData, out List<Tag<T>> tags)
        {
            TagBuilder<T> blockTagBuilder = new TagBuilder<T>(tagHolderGetter);
            DependencyDataResolver<TagData, Tag<T>> dependencyResolver =
                new DependencyDataResolver<TagData, Tag<T>>(unresolvedTagData, blockTagBuilder);
            if (dependencyResolver.TryBuildResolvedData(out Dictionary<StringID, Tag<T>>? resolvedData, false))
            {
                tags = resolvedData.Values.ToList();
            }
            else
            {
                throw new Exception("");
            }
        }

        public TagLoader(string tagHolderType, TagBuilder<T>.TagHolderGetter tagHolderGetter, Action<TagManager<T>> tagManagerConsumer)
        {
            this.tagHolderType = tagHolderType;
            this.tagHolderGetter = tagHolderGetter;
            consumeTagManager = tagManagerConsumer;
        }

        private readonly string tagHolderType;

        private readonly TagBuilder<T>.TagHolderGetter tagHolderGetter;

        private readonly Action<TagManager<T>> consumeTagManager;
    }
}
