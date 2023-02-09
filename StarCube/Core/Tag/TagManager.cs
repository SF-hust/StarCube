using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;

using StarCube.Core.Tag.Attributes;

namespace StarCube.Core.Tag
{
    /*
    public class TagManager<T>
        where T : class
    {
        /// <summary>
        /// 此类别下所有 Tag，按 id 的命名空间优先顺序排序
        /// </summary>
        public readonly ImmutableArray<Tag<T>> tags;

        private readonly ImmutableDictionary<T, ImmutableHashSet<Tag<T>>> elementToTags;

        internal TagManager(ImmutableArray<Tag<T>> tags, ImmutableDictionary<T, ImmutableHashSet<Tag<T>>> elementToTags)
        {
            this.tags = tags;
            this.elementToTags = elementToTags;
        }

        public ImmutableHashSet<Tag<T>> GetTags(T element)
        {
            if(elementToTags.TryGetValue(element, out var tag))
            {
                return tag;
            }
            return ImmutableHashSet<Tag<T>>.Empty;
        }

        public bool ElementHasTag(T element, Tag<T> tag)
        {
            return elementToTags.TryGetValue(element, out var tagsOfElement) && tagsOfElement.Contains(tag);
        }

        public class Builder
        {
            private readonly Dictionary<StarCube.Resource.StringID, List<FieldInfo>> tagDeclarationsInCode = new Dictionary<StarCube.Resource.StringID, List<FieldInfo>>();

            private readonly Dictionary<StarCube.Resource.StringID, Tag<T>> tags = new Dictionary<StarCube.Resource.StringID, Tag<T>>();

            private readonly Dictionary<StarCube.Resource.StringID, TagBuilder<T>> builders = new Dictionary<Resource.StringID, TagBuilder<T>>();

            private void GetTagDeclaresInCode(Type type, string modid)
            {
                foreach(FieldInfo field in type.GetFields())
                {

                    if (!(Attribute.GetCustomAttribute(field, typeof(TagAttribute)) is TagAttribute tagAttribute))
                    {
                        continue;
                    }
                    if(!field.IsStatic)
                    {
                        throw new Exception($"field {field.Name} in class {type.FullName} with [Tag] must be static");
                    }
                    if (!field.IsPublic)
                    {
                        throw new Exception($"field {field.Name} in class {type.FullName} with [Tag] must be public");
                    }
                    if (field.FieldType != typeof(Tag<T>))
                    {
                        throw new Exception($"field {field.Name} in class {type.FullName} with [Tag] must be type of Tag<T>");
                    }
                    string name = tagAttribute.name;
                    StarCube.Resource.StringID id = StarCube.Resource.StringID.Create(modid, name);
                    if (tagDeclarationsInCode.TryGetValue(id, out List<FieldInfo> fields))
                    {
                        fields.Add(field);
                    }
                    else
                    {
                        tagDeclarationsInCode.Add(id, new List<FieldInfo> { field });
                    }
                }
            }

            public void CollectTagDeclaresInAssembly(Assembly assembly)
            {
                foreach(Type type in assembly.DefinedTypes)
                {
                    if (!(Attribute.GetCustomAttribute(type, typeof(TagDeclareAttribute)) is TagDeclareAttribute attribute) || attribute.targetClass != typeof(T))
                    {
                        continue;
                    }
                    if (!type.Attributes.HasFlag(TypeAttributes.Sealed & TypeAttributes.Abstract))
                    {
                        throw new Exception($"class {type.FullName} with [TagDeclare] must be static");
                    }
                    string modid = attribute.modid;
                    GetTagDeclaresInCode(type, modid);
                }
            }

            private void SetTagDeclareValue()
            {
                foreach (var pair in tagDeclarationsInCode)
                {
                    StarCube.Resource.StringID id = pair.Key;
                    if (!tags.TryGetValue(id, out Tag<T> tag))
                    {
                        tag = new Tag<T>(id, ImmutableArray<T>.Empty);
                        tags[id] = tag;
                    }
                    foreach (FieldInfo field in pair.Value)
                    {
                        field.SetValue(null, tag);
                    }
                }
            }

            private Builder()
            {
            }

            public Builder Create()
            {
                return new Builder();
            }

            public Builder Add(StarCube.Resource.StringID tagId, TagData tagData, string source)
            {
                if (builders.TryGetValue(tagId, out TagBuilder<T> builder))
                {
                    builder.AddFromTagData(tagData, source);
                }
                else
                {
                    builders.Add(tagId, TagBuilder<T>.Create(tagId).AddFromTagData(tagData, source));
                }
                return this;
            }

            public TagManager<T>? TryBuild(TagBuilder<T>.ElementGetter elementGetter)
            {
                foreach (var pair in builders)
                {
                    if (tags.ContainsKey(pair.Key))
                    {
                        continue;
                    }
                    if (pair.Value.TryBuild(elementGetter,
                        (id) => GetTag(id, elementGetter),
                        out Tag<T>? tag,
                        out List<(StarCube.Resource.StringID, TagData.Entry.EntryType, string)>? missingDependencies))
                    {
                        tags.Add(pair.Key, tag);
                    }
                }
                SetTagDeclareValue();
                return new TagManager<T>(tags.Values.ToImmutableArray(), ImmutableDictionary<T, ImmutableHashSet<Tag<T>>>.Empty);
            }

            private Tag<T>? GetTag(StarCube.Resource.StringID id, TagBuilder<T>.ElementGetter elementGetter)
            {
                if (tags.TryGetValue(id, out Tag<T> tag))
                {
                    return tag;
                }
                else
                {
                    if (!builders.TryGetValue(id, out TagBuilder<T> depBuilder))
                    {
                        return null;
                    }
                    if (depBuilder.State == TagBuilder<T>.BuildState.BeforeBuild)
                    {
                        throw new Exception("Found circular reference in tags");
                    }

                    Debug.Assert(depBuilder.State == TagBuilder<T>.BuildState.BeforeBuild);

                    if (depBuilder.TryBuild(
                        elementGetter,
                        (id) => GetTag(id, elementGetter),
                        out Tag<T>? depTag,
                        out List<(StarCube.Resource.StringID, TagData.Entry.EntryType, string)>? missingDependencies)
                    )
                    {
                        return depTag;
                    }
                    depTag = new Tag<T>(id, ImmutableArray<T>.Empty);
                    return null;
                }
            }
        }
    }
    */
}
