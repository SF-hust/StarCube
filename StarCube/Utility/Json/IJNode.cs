namespace StarCube.Utility.Json
{
    public interface IJNode
    {
        public bool IsValue { get; }

        public bool IsArray { get; }

        public bool IsJson { get; }

        public IJValue AsValue { get; }

        public IJArray AsArray { get; }

        public IJson AsJson { get; }
    }
}
