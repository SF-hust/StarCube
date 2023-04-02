namespace StarCube.Game.Blocks
{
    public readonly struct BlockProperties
    {
        public readonly bool air;

        public readonly bool solid;

        public readonly float hardness;

        public readonly float strength;

        public BlockProperties(
            bool air,
            bool solid,
            float hardness,
            float strength)
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

            public Builder Hardness(float hardness)
            {
                this.hardness = hardness;
                return this;
            }

            public Builder Strength(float strength)
            {
                this.strength = strength;
                return this;
            }

            private bool air = false;

            private bool solid = true;

            private float hardness = 0.0f;

            private float strength = 0.0f;
        }
    }
}
