using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using LiteDB;

using StarCube.Utility;
using StarCube.Data.DependencyResolver;
using StarCube.Data.Loading;
using System.Diagnostics.CodeAnalysis;

namespace StarCube.Core.Component.Data
{
    public readonly struct ComponentDefinitionEntry
    {
        public ComponentDefinitionEntry(StringID typeID, StringID variantID, BsonDocument bson)
        {
            this.typeID = typeID;
            this.variantID = variantID;
            this.bson = bson;
        }

        public readonly StringID typeID;
        public readonly StringID variantID;
        public readonly BsonDocument bson;
    }


    public class RawComponentDefinitionData : IUnresolvedData<RawComponentDefinitionData>
    {
        public static readonly StringID DataRegistry = StringID.Create(Constants.DEFAULT_NAMESPACE, "component_def");

        public static readonly IDataReader<RawComponentDefinitionData> DataReader = new DataReaderWrapper<RawComponentDefinitionData, JObject>(RawDataReaders.JSON, TryParseFromJson);

        public static bool TryParseFromJson(JObject json, StringID id, [NotNullWhen(true)] out RawComponentDefinitionData? data)
        {
            data = null;
            return false;
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
