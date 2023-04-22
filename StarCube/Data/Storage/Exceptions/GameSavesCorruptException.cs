using System;

namespace StarCube.Data.Storage.Exceptions
{
    public class GameSavesCorruptException : Exception
    {
        public GameSavesCorruptException()
        {
        }

        public GameSavesCorruptException(string message)
            : base(message)
        {
        }
    }
}
