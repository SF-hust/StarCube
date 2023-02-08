using System;
using System.Collections.Generic;
using System.Text;

namespace StarCube.Data.Exception
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
