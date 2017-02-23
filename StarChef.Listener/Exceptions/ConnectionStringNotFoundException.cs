namespace StarChef.Listener.Exceptions
{
    /// <summary>
    ///     Raised when a connection string is not found
    /// </summary>
    internal class ConnectionStringNotFoundException : ListenerException
    {
        public ConnectionStringNotFoundException(string message) : base(message)
        {
        }
    }
}