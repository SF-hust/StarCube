namespace StarCube.Data.Exceptions
{
    public class DataLoadException : System.Exception
    {
        public DataLoadException()
        {
        }

        public DataLoadException(string message)
            : base(message)
        {
        }
    }
}
