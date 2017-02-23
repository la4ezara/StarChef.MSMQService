using Fourth.Orchestration.Messaging;
using StarChef.Listener.Exceptions;

namespace StarChef.Listener
{
    interface IMessagingHandlersFactory
    {
        /// <summary>
        /// Create a handler for message of the given type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="NotSupportedMessageException">Raised when message type is not supported</exception>
        /// <returns></returns>
        IMessageHandler CreateHandler<T>();
    }
}