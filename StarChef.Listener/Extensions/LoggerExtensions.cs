using System;
using log4net;
using StarChef.Listener.Exceptions;

namespace StarChef.Listener.Extensions
{
    public static class LoggerExtensions
    {
        public static void EventReceived(this ILog logger, string trackingId, object payload)
        {
            logger.InfoFormat("Event received. [{0}] {1}", trackingId, payload.ToJson());
        }

        public static void InvalidModel(this ILog logger, string trackingId, object payload, string errors)
        {
            logger.ErrorFormat("Invalid payload: {0}. [{1}] {2}", errors, trackingId, payload.ToJson());
        }

        public static void ListenerException(this ILog logger, ListenerException exception, string trackingId, object data)
        {
            logger.Error(string.Format("Exception of type {0} is occurred with message: {1}", exception.GetType().Name, exception.Message));
            logger.Error(string.Format("Exception data: [{0}]{1}", trackingId, data.ToJson()));
        }

        public static void Processed(this ILog logger, string trackingId, object payload)
        {
            logger.InfoFormat("Event processed. [{0}] {1}", trackingId, payload.ToJson());
        }

        public static void MessageSent(this ILog logger, object message)
        {
            logger.InfoFormat("Message '{0}' sent: {1}", message.GetType().Name, message.ToJson());
        }

        public static void DatabaseError(this ILog logger, Exception exception)
        {
            logger.ErrorFormat("Database operation failed. [{0}] '{1}'", exception.GetType(), exception.Message);
            logger.Error(exception);
        }

        public static void EventDisabledForOrganization(this ILog logger, object payload)
        {
            logger.InfoFormat("Processing of the event is disabled for organization.");
        }
    }
}