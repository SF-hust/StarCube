using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace StarCube.Mod
{
    public class ModLoader
    {
        public static void LoadModAssemblyInZip(string zipPath)
        {
            ZipArchive zip = ZipFile.OpenRead(zipPath);

            List<string> tempFilenames = new List<string>();

            foreach (var entry in zip.Entries)
            {
                if (entry.FullName.StartsWith("libs/", StringComparison.OrdinalIgnoreCase) && entry.FullName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    string temp = Path.GetTempFileName();
                    entry.ExtractToFile(temp, true);
                    tempFilenames.Add(temp);
                }
            }
        }
    }
}
