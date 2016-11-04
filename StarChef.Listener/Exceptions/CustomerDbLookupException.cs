using System;

namespace StarChef.Listener.Exceptions
{
    internal class CustomerDbLookupException : ListenerException
    {
        public CustomerDbLookupException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public CustomerDbLookupException(string message) : base(message)
        {
        }
    }
}