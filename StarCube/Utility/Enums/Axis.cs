namespace StarCube.Utility.Enums
{
    public enum Axis
    {
        None = 0x0,
        X = 0x1,
        Z = 0x2,
        Y = 0x4,
    }

    public static class AxisExtension
    {
        public const string X = "x";
        public const string Y = "y";
        public const string Z = "z";

        public static Axis Parse(string axisString)
        {
            if (axisString == X)
            {
                return Axis.X;
            }
            if (axisString == Y)
            {
                return Axis.Y;
            }
            if (axisString == Z)
            {
                return Axis.Z;
            }
            return Axis.None;
        }

        public static string ToAxisString(this Axis axis)
        {
            if (axis == Axis.X)
            {
                return X;
            }
            if (axis == Axis.Y)
            {
                return Y;
            }
            if (axis == Axis.Z)
            {
                return Z;
            }
            return string.Empty;
        }

    }
}
