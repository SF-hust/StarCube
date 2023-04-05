using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using StarCube.Data.DependencyDataResolver;

namespace StarCube.Core.Components.Data
{
    public class ComponentDefinitionDataBuilder : IResolvedDataBuilder<RawComponentDefinitionData, ComponentDefinitionData>
    {
        public bool BuildResolvedData(
            RawComponentDefinitionData unresolvedData,
            IResolvedDataBuilder<RawComponentDefinitionData, ComponentDefinitionData>.ResolvedDataGetter getResolvedData,
            [NotNullWhen(true)] out ComponentDefinitionData? resolvedData)
        {
            resolvedData = null;

            if(unresolvedData.parent == null)
            {
                resolvedData = new ComponentDefinitionData(unresolvedData.id, unresolvedData.entries);
                return true;
            }

            if(!getResolvedData(unresolvedData.parent, out ComponentDefinitionData? parent))
            {
                return false;
            }

            List<ComponentDefinitionEntry> entries;
            if(unresolvedData.entries.Count == 0)
            {
                entries = parent.entries;
            }
            else
            {
                entries = new List<ComponentDefinitionEntry>();
                entries.AddRange(parent.entries);
                entries.AddRange(unresolvedData.entries);
            }

            resolvedData = new ComponentDefinitionData(unresolvedData.id, entries);
            return true;
        }
    }
}
