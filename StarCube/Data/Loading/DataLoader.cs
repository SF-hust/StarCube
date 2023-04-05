using System.Collections.Generic;

using StarCube.Utility;

namespace StarCube.Data.Loading
{
    public abstract class DataLoader : IStringID
    {
        public abstract void Run(DataLoadingContext context);

        public IEnumerable<StringID> Dependencies => dependencies;
        public IEnumerable<StringID> Followers => followers;

        StringID IStringID.ID => id;

        public DataLoader(StringID id, bool reloadable)
        {
            this.id = id;
            this.reloadable = reloadable;
            dependencies = new List<StringID>();
            followers = new List<StringID>();
        }

        public readonly StringID id;

        public readonly bool reloadable;

        protected List<StringID> dependencies;

        protected List<StringID> followers;
    }
}
