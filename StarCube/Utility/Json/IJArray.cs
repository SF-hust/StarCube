using System;
using System.Collections.Generic;

namespace StarCube.Utility.Json
{
    public interface IJArray : IJNode
    {
        public int Length { get; }

        public IEnumerable<IJNode> Nodes { get; }

        public IEnumerable<bool> GetBools();

        public IEnumerable<int> GetInt32s();

        public IEnumerable<uint> GetUInt32s();

        public IEnumerable<long> GetInt64s();

        public IEnumerable<ulong> GetUInt64s();

        public IEnumerable<float> GetFloats();

        public IEnumerable<double> GetDoubles();

        public IEnumerable<Guid> GetGuids();

        public IEnumerable<string> GetStrings();

        public void Add(bool value);

        public void Add(int value);

        public void Add(uint value);

        public void Add(long value);

        public void Add(ulong value);

        public void Add(float value);

        public void Add(double value);

        public void Add(Guid value);

        public void Add(string value);

        public void Add(IJNode value);

        public void Add(IJValue value);

        public void Add(IJArray value);

        public void Add(IJson value);
    }
}
