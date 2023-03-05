using System;
using System.Collections.Generic;
using System.IO;

namespace StarCube.Data.Loading
{
    public class DataPackedDataProvider// : IDataProvider
    {
        public DataPackedDataProvider(string gameDirectory)
        {
            this.gameDirectory = gameDirectory;
            modDirectory = Path.Combine(gameDirectory, "mods/");
            dataDirectory = Path.Combine(gameDirectory, "data/");
        }

        public string GameDirectory
        {
            get => gameDirectory;
            set
            {
                gameDirectory = value;
                modDirectory = Path.Combine(gameDirectory, "mods/");
                dataDirectory = Path.Combine(gameDirectory, "data/");
            }
        }

        public string ModDirectory => modDirectory;

        public string DataDirectory => dataDirectory;

        // IDataProvider 实现 start
        public IEnumerable<IDataProvider.DataEntry> EnumerateData(StringID dataRegistry)
        {
            throw new NotImplementedException();
        }

        public bool TryGet(StringID registry, StringID id, out IDataProvider.DataEntry entry)
        {
            string path = Path.Combine(dataDirectory, registry.path, id.ToString());
            FileStream fileStream = File.OpenRead(path);
            entry = new IDataProvider.DataEntry(id, fileStream);
            return true;
        }

        public void Refresh()
        {
        }

        // IDataProvider 实现 end

        private string gameDirectory;

        private string modDirectory;

        private string dataDirectory;



    }
}
