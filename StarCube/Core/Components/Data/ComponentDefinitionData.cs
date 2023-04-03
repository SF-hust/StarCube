using System.Collections.Generic;

using StarCube.Utility;

namespace StarCube.Core.Components.Data
{
    public sealed class ComponentDefinitionData : IStringID
    {
        StringID IStringID.ID => id;

        public ComponentDefinitionData(StringID id, List<ComponentDefinitionEntry> entries)
        {
            this.id = id;
            this.entries = entries;
        }

        public readonly StringID id;

        public readonly List<ComponentDefinitionEntry> entries;
    }
}
