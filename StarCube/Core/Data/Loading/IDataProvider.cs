using System.Collections.Generic;
using System.IO;

using StarCube.Resource;

namespace StarCube.Core.Data.Loading
{
    public interface IDataProvider : IEnumerable<IDataProvider.Data>
    {
        public readonly struct Data
        {
            public readonly StringID id;
            public readonly Stream dataStream;
        }

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

        public IEnumerable<DataEntry> EnumData(StringID dataRegistry);

        public bool TryLoad(StringID registry, StringID id, out DataEntry entry);
    }
}
