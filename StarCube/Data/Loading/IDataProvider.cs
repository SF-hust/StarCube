using System.Collections.Generic;
using System.IO;

namespace StarCube.Data.Loading
{
    public interface IDataProvider
    {
        public readonly struct DataEntry
        {
            public readonly StringID id;
            public readonly Stream dataStream;

            public DataEntry(StringID id, Stream dataStream)
            {
                this.id = id;
                this.dataStream = dataStream;
            }
        }

        public void Refresh();

        public IEnumerable<DataEntry> EnumerateData(StringID dataRegistry);

        public bool TryGet(StringID dataRegistry, StringID id, out DataEntry entry);
    }
}
