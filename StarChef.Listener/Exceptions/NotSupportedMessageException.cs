namespace StarChef.Listener.Exceptions
{
    internal class NotSupportedMessageException : ListenerException
    {
        public NotSupportedMessageException(string message) : base(message)
        {
        }
    }
}