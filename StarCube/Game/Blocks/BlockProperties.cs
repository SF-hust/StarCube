namespace StarCube.Game.Blocks
{
    public class BlockProperties
    {
        public readonly bool air;

        public readonly bool solid;

        public readonly double hardness;

        public readonly double strength;

        public BlockProperties(
            bool air,
            bool solid,
            double hardness,
            double strength)
        {
            this.air = air;
            this.solid = solid;
            this.hardness = hardness;
            this.strength = strength;
        }

        public class Builder
        {
            public static Builder Create()
            {
                return new Builder();
            }
            public BlockProperties Build()
            {
                return new BlockProperties(air, solid, hardness, strength);
            }

            public Builder Air()
            {
                air = true;
                solid = false;
                return this;
            }

            public Builder Solid()
            {
                solid = true;
                air = false;
                return this;
            }

            public Builder Hardness(double hardness)
            {
                this.hardness = hardness;
                return this;
            }

            public Builder Strength(double strength)
            {
                this.strength = strength;
                return this;
            }

            private bool air = false;

            private bool solid = true;

            private double hardness = 0.0;

            private double strength = 0.0;
        }
    }
}
