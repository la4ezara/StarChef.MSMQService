using System;

namespace StarChef.Listener.Exceptions
{
    internal class ConnectionStringLookupException : ListenerException
    {
        public ConnectionStringLookupException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ConnectionStringLookupException(string message) : base(message)
        {
        }
    }
}