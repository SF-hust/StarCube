using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Newtonsoft.Json.Linq;

using StarCube.Utility;
using StarCube.Data;

namespace StarCube.Core.Registry.Data
{
    public class RegistryData
    {
        public static StringID DataRegistry = Registry.RegistryRegistry;

        public static bool TryParseFromJson(JObject json, StringID id, [NotNullWhen(true)] out RegistryData? registryData)
        {
            registryData = null;
            List<string> entries= new List<string>();
            if(!json.TryGetArray("entries", out JArray? entriesJArray) || !entriesJArray.ToStringArray(out string[] entriesArray))
            {
                return false;
            }

            foreach (string entry in entriesArray)
            {
                if(!StringID.IsValidName(entry))
                {
                    return false;
                }

                entries.Add(entry);
            }

            registryData = new RegistryData(id, entries);
            return true;
        }

        public RegistryData(StringID id, List<string> entries)
        {
            this.id = id;
            this.entries = entries;
        }

        /// <summary>
        /// RegistryData 文件的 id
        /// </summary>
        public readonly StringID id;

        public readonly List<string> entries = new List<string>();
    }
}
