using System;

namespace StarCube.Utility.Json
{
    public interface IJValue : IJNode
    {
        public bool IsBoolean { get; }

        public bool IsInt { get; }

        public bool IsFloat { get; }

        public bool IsGuid { get; }

        public bool IsString { get; }

        public bool AsBoolean { get; }

        public int AsInt32 { get; }

        public long AsInt64 { get; }

        public float AsFloat { get; }

        public double AsDouble { get; }

        public Guid AsGuid { get; }

        public string AsString { get; }
    }
}
