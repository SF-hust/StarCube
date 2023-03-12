using System;

namespace StarCube.Utility.Json
{
    public interface IJson : IJNode
    {
        public interface IFactory
        {
            public IJson Create();

            public IJson Parse(string jsonString);
        }

        public static IFactory factory = null!;

        public static IJson Create()
        {
            return factory.Create();
        }

        public static IJson FromString(string jsonString)
        {
            return factory.Parse(jsonString);
        }


        public bool TryGetBoolean(string key, out bool value);

        public bool TryGetInt32(string key, out int value);

        public bool TryGetUInt32(string key, out uint value);

        public bool TryGetInt64(string key, out long value);

        public bool TryGetUInt64(string key, out ulong value);

        public bool TryGetFloat(string key, out float value);

        public bool TryGetDouble(string key, out double value);

        public bool TryGetGuid(string key, out Guid value);

        public bool TryGetString(string key, out string value);

        public bool TryGetNode(string key, out IJNode value);

        public bool TryGetValue(string key, out IJValue value);

        public bool TryGetArray(string key, out IJArray value);

        public bool TryGetJson(string key, out IJson value);

        public bool TryAdd(string key, bool value);

        public bool TryAdd(string key, int value);

        public bool TryAdd(string key, uint value);

        public bool TryAdd(string key, long value);

        public bool TryAdd(string key, ulong value);

        public bool TryAdd(string key, float value);

        public bool TryAdd(string key, double value);

        public bool TryAdd(string key, Guid value);

        public bool TryAdd(string key, string value);

        public bool TryAdd(string key, IJNode value);

        public bool TryAdd(string key, IJValue value);

        public bool TryAdd(string key, IJArray value);

        public bool TryAdd(string key, IJson value);
    }
}
