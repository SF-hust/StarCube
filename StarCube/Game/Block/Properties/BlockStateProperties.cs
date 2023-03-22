using StarCube.Utility;
using StarCube.Utility.Enums;
using StarCube.BootStrap.Attributes;
using StarCube.Core.State.Property;
using StarCube.Game.Block.Enums;

namespace StarCube.Game.Block.Properties
{
    [ConstructInBootStrap]
    public static class BlockStateProperties
    {
        public static readonly EnumStateProperty<Axis> AXIS_XYZ = EnumStateProperty<Axis>.Create(StringID.Create(Constants.DEFAULT_NAMESPACE, "axis_xyz"), new Axis[3] { Axis.X, Axis.Y, Axis.Z });

        public static readonly EnumStateProperty<Axis> AXIS_XZ = EnumStateProperty<Axis>.Create(StringID.Create(Constants.DEFAULT_NAMESPACE, "axis_xz"), new Axis[2] { Axis.X, Axis.Z });

        public static readonly BooleanStateProperty FENCE_NORTH = BooleanStateProperty.Create(StringID.Create(Constants.DEFAULT_NAMESPACE, "fence_north"));
        public static readonly BooleanStateProperty FENCE_SOUTH = BooleanStateProperty.Create(StringID.Create(Constants.DEFAULT_NAMESPACE, "fence_south"));
        public static readonly BooleanStateProperty FENCE_EAST = BooleanStateProperty.Create(StringID.Create(Constants.DEFAULT_NAMESPACE, "fence_east"));
        public static readonly BooleanStateProperty FENCE_WEST = BooleanStateProperty.Create(StringID.Create(Constants.DEFAULT_NAMESPACE, "fence_west"));

        public static readonly EnumStateProperty<SlabPart> SLAB_PART = EnumStateProperty<SlabPart>.Create(StringID.Create(Constants.DEFAULT_NAMESPACE, "slab_part"));

        public static readonly IntegerStateProperty AGE4 = IntegerStateProperty.Create(StringID.Create(Constants.DEFAULT_NAMESPACE, "age_4"), 0, 3);
    }
}
