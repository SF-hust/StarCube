namespace StarCube.Data.Exception
{
    public class DataLoadingException : System.Exception
    {
        public DataLoadingException()
        {
        }

        public DataLoadingException(string message) : base(message)
        {
        }
    }
}
