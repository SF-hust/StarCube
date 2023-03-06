using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using StarCube.Utility;
using StarCube.Utility.Enums;
using StarCube.BootStrap.Attributes;
using StarCube.Data;

namespace StarCube.Core.State.Property
{
    [ConstructInBootStrap]
    public class StateProperties
    {
        public static readonly EnumStateProperty<Axis> AXIS = EnumStateProperty<Axis>.Create(StringID.Create(Constants.DEFAULT_NAMESPACE, "axis"));


        public static void Register(StateProperty property)
        {
            if(!stateProperties.TryAdd(property.id, property))
            {
                throw new ArgumentException($"re-register state property \"{property.id}\"");
            }
        }

        public static bool TryGet(StringID id, [NotNullWhen(true)] out StateProperty? property)
        {
            return stateProperties.TryGetValue(id, out property);
        }


        private static readonly ConcurrentDictionary<StringID, StateProperty> stateProperties = new ConcurrentDictionary<StringID, StateProperty>();

        static StateProperties()
        {
            Register(AXIS);
        }
    }
}
