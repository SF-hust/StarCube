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
        public static readonly EnumStateProperty<Axis> AXIS = EnumStateProperty<Axis>.Create(StringID.Create(Constants.DEFAULT_NAMESPACE, "axis"), new Axis[3] {Axis.X, Axis.Y, Axis.Z});

        public static readonly BooleanStateProperty NORTH = BooleanStateProperty.Create(StringID.Create(Constants.DEFAULT_NAMESPACE, "north"));
        public static readonly BooleanStateProperty SOUTH = BooleanStateProperty.Create(StringID.Create(Constants.DEFAULT_NAMESPACE, "south"));
        public static readonly BooleanStateProperty EAST = BooleanStateProperty.Create(StringID.Create(Constants.DEFAULT_NAMESPACE, "east"));
        public static readonly BooleanStateProperty WEST = BooleanStateProperty.Create(StringID.Create(Constants.DEFAULT_NAMESPACE, "west"));

        public static readonly IntegerStateProperty AGE4 = IntegerStateProperty.Create(StringID.Create(Constants.DEFAULT_NAMESPACE, "age4"), 0, 3);
    }
}
