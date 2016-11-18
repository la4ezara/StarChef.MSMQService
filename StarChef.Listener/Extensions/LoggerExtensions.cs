using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using StarChef.Listener.Exceptions;
using StarChef.Orchestrate.Models.TransferObjects;

namespace StarChef.Listener.Extensions
{
    public static class LoggerExtensions
    {
        public static void EventReceived(this ILog logger, string trackingId, object payload)
        {
            logger.InfoFormat("The event '{0}' received. [{1}] {2}", payload.GetType(), trackingId, payload.ToJson());
        }

        public static void InvalidModel(this ILog logger, string trackingId, object payload, string errors)
        {
            logger.ErrorFormat("Invalid message '{0}': {1}. [{2}] {3}", payload.GetType(), errors, trackingId, payload.ToJson());
        }

        public static void ListenerException(this ILog logger, ListenerException exception, string trackingId, object data)
        {
            logger.Error(string.Format("Exception of type {0} is occurred with message: {1}. Tracking ID: {2}", exception.GetType(), exception.Message));
            logger.Error(string.Format("Exception data: [{0}]{1}", trackingId, data.ToJson()));
        }

        public static void Processed(this ILog logger, string trackingId, object payload)
        {
            logger.InfoFormat("Message '{0}' processed. [{1}] {2}", payload.GetType(), trackingId, payload.ToJson());
        }

        public static void DatabaseError(this ILog logger, Exception exception)
        {
            logger.ErrorFormat("Database operation failed. [{0}] '{1}'", exception.GetType(), exception.Message);
            logger.Error(exception);
        }
    }
}
