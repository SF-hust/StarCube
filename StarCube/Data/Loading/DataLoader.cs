using System;
using System.Collections.Generic;

using StarCube.Utility;

namespace StarCube.Data.Loading
{
    public abstract class DataLoader
    {
        public abstract void Run(DataLoadingContext context);

        public IEnumerable<StringID> Follows => follows;
        public IEnumerable<StringID> Followers => followers;

        public DataLoader(StringID id)
        {
            this.id = id;
            follows = Array.Empty<StringID>();
            followers = Array.Empty<StringID>();
        }

        public readonly StringID id;

        protected StringID[] follows;

        protected StringID[] followers;
    }
}
