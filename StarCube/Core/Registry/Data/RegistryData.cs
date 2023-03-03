using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Newtonsoft.Json.Linq;

using StarCube.Utility;
using StarCube.Data;

namespace StarCube.Core.Registry.Data
{
    public class RegistryData
    {
        public static bool TryParseFromJson(StringID registry, JObject json, [NotNullWhen(true)] out RegistryData? registryData)
        {
            registryData = null;
            List<StringID> entries= new List<StringID>();
            if(!json.TryGetArray("entries", out JArray? jArray) || jArray.ToStringArray(out string[] array))
            {
                return false;
            }

            foreach (string str in array)
            {
                if(!StringID.TryParse(str, out StringID id))
                {
                    return false;
                }
                entries.Add(id);
            }

            registryData = new RegistryData(registry, entries);
            return true;
        }

        public RegistryData(StringID registry, List<StringID> entries)
        {
            this.registry = registry;
            this.entries = entries;
        }

        public readonly StringID registry;

        public IEnumerable<StringID> Entries => entries;

        private List<StringID> entries = new List<StringID>();
    }
}
