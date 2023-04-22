namespace StarCube.Data.Exceptions
{
    public class DataParseException : DataLoadException
    {
        public DataParseException()
            : base("parse failed")
        {
        }

        public DataParseException(string message)
            : base("parse failed : " + message)
        {

        }
    }
}
