using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Newtonsoft.Json.Linq;

using StarCube.Utility;
using StarCube.Data.DependencyResolver;
using StarCube.Data.Loading;

namespace StarCube.Core.Components.Data
{
    public readonly struct ComponentDefinitionEntry
    {
        public static bool TryParseFromJson(JObject json, out ComponentDefinitionEntry entry)
        {
            entry = new ComponentDefinitionEntry();

            if(!json.TryGetStringID("type", out StringID typeID))
            {
                return false;
            }

            if (!json.TryGetStringID("variant", out StringID variantID))
            {
                return false;
            }

            if (!json.TryGetJObject("args", out JObject? argObject))
            {
                argObject = new JObject();
            }

            entry = new ComponentDefinitionEntry(typeID, variantID, argObject);
            return true;
        }

        public ComponentDefinitionEntry(StringID typeID, StringID variantID, JObject args)
        {
            this.typeID = typeID;
            this.variantID = variantID;
            this.args = args;
        }

        public readonly StringID typeID;
        public readonly StringID variantID;
        public readonly JObject args;
    }


    public sealed class RawComponentDefinitionData : IUnresolvedData<RawComponentDefinitionData>
    {
        public static readonly StringID DataRegistry = StringID.Create(Constants.DEFAULT_NAMESPACE, "component");

        public static readonly IDataReader<RawComponentDefinitionData> DataReader = new DataReaderWrapper<RawComponentDefinitionData, JObject>(RawDataReaders.JSON, TryParseFromJson);

        public static bool TryParseFromJson(JObject json, StringID id, [NotNullWhen(true)] out RawComponentDefinitionData? data)
        {
            data = null;

            if (!json.TryGetStringID("parent", out StringID? parentID))
            {
                parentID = null;
            }

            List<ComponentDefinitionEntry> entries = new List<ComponentDefinitionEntry>();
            if (json.TryGetArray("components", out JArray? componentArray))
            {
                foreach (JToken token in componentArray)
                {
                    if(!(token is JObject componentObject))
                    {
                        return false;
                    }

                    if(!ComponentDefinitionEntry.TryParseFromJson(componentObject, out ComponentDefinitionEntry entry))
                    {
                        return false;
                    }

                    entries.Add(entry);
                }
            }

            data = new RawComponentDefinitionData(id, parentID, entries);
            return true;
        }


        StringID IStringID.ID => id;

        public RawComponentDefinitionData UnresolvedData => this;
        public IEnumerable<StringID> RequiredDependencies => parents;
        public IEnumerable<StringID> OptionalDependencies => Array.Empty<StringID>();

        public RawComponentDefinitionData(StringID id, StringID? parent, List<ComponentDefinitionEntry> entries)
        {
            this.id = id;
            this.parent = parent;
            this.entries = entries;
            parents = parent == null ? Array.Empty<StringID>() : new StringID[1] { parent };
        }

        public readonly StringID id;

        public readonly StringID? parent;

        public readonly List<ComponentDefinitionEntry> entries;

        private readonly StringID[] parents;
    }
}
