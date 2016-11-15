using System;

namespace StarChef.Listener.Exceptions
{
    internal class DatabaseException : ListenerException
    {
        public DatabaseException(Exception exception) : base("Database operation is failed.", exception)
        {
        }
    }
}