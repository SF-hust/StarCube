using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using Nett;

namespace StarCube.Utility
{
    public static class TomlHelper
    {
        public static bool TryReadFromStream(Stream stream, long length, [NotNullWhen(true)] out TomlTable? toml)
        {
            bool sucess = false;
            try
            {
                toml = Toml.ReadStream(stream);
                sucess = true;
            }
            catch(Exception)
            {
                toml = null;
            }

            return sucess;
        }
    }
}
