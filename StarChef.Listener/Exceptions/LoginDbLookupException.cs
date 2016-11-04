using System;

namespace StarChef.Listener.Exceptions
{
    internal class LoginDbLookupException : ListenerException
    {
        public LoginDbLookupException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public LoginDbLookupException(string message) : base(message)
        {
        }
    }

    internal class DataNotSavedException : ListenerException
    {
        public DataNotSavedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public DataNotSavedException(string message) : base(message)
        {
        }
    }
}