using System;
using System.Collections.Generic;
using System.Linq;

using StarCube.Utility;
using StarCube.Data.Loading;
using StarCube.Data.Provider;
using StarCube.Data.DependencyResolver;

namespace StarCube.Core.Tag.Data
{
    public class TagDataLoader<T> : IDataLoader
        where T : class, ITagHolder<T>, IStringID
    {
        public void Run(IDataProvider dataProvider)
        {
            List<TagData> tagDataList = dataProvider.EnumerateData(TagData.DataRegistry, tagHolderType, TagData.DataReader);

            TagBuilder<T> blockTagBuilder = new TagBuilder<T>(tagHolderGetter);
            DependencyDataResolver<TagData, Tag<T>> dependencyResolver =
                new DependencyDataResolver<TagData, Tag<T>>(tagDataList, blockTagBuilder);

            dependencyResolver.TryBuildResolvedData(out Dictionary<StringID, Tag<T>>? resolvedData, out List<TagData> failedTagDataList, false);
            List<Tag<T>> tags = resolvedData.Values.ToList();

            TagManager<T> tagManager = new TagManager<T>(tags);
            consumeResult(tagManager, (from tagData in failedTagDataList select tagData.id).ToList());
        }

        public TagDataLoader(string tagHolderType, TagBuilder<T>.TagHolderGetter tagHolderGetter, Action<TagManager<T>, List<StringID>> resultConsumer)
        {
            this.tagHolderType = tagHolderType;
            this.tagHolderGetter = tagHolderGetter;
            consumeResult = resultConsumer;
        }

        private readonly string tagHolderType;

        private readonly TagBuilder<T>.TagHolderGetter tagHolderGetter;

        private readonly Action<TagManager<T>, List<StringID>> consumeResult;
    }
}
