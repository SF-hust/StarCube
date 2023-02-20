using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

using StarCube.Resource;

namespace StarCube.Core.Data.Loading
{
    public class DataProvider : IDataProvider
    {
        public DataProvider(string gameDirectory)
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
        public IEnumerable<IDataProvider.DataEntry> EnumData(StringID dataRegistry)
        {
            throw new NotImplementedException();
        }

        public bool TryLoad(StringID registry, StringID id, out IDataProvider.DataEntry entry)
        {
            string path = Path.Combine(dataDirectory, registry.path, id.ToString());
            FileStream fileStream = File.OpenRead(path);
            entry = new IDataProvider.DataEntry(id, fileStream);
            return true;
        }
        // IDataProvider 实现 end

        private string gameDirectory;

        private string modDirectory;

        private string dataDirectory;



        public IEnumerator<IDataProvider.Data> GetEnumerator()
        {
            throw new NotImplementedException();
        }
        public void Refresh()
        {
            throw new NotImplementedException();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
