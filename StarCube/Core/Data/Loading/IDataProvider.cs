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

        public void Refresh();
    }
}
