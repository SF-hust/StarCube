namespace StarCube.Core.Data.Exception
{
    public class DataParseException : DataLoadingException
    {
        public DataParseException() : base("parse failed")
        {
        }

        public DataParseException(string message) : base("parse failed : " + message)
        {

        }
    }
}
