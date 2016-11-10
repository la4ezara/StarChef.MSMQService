using System;
using System.Runtime.Serialization;

namespace StarChef.Listener.Exceptions
{
    internal class ListenerException : Exception
    {
        public ListenerException()
        {
        }

        public ListenerException(string message) : base(message)
        {
        }

        public ListenerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ListenerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}