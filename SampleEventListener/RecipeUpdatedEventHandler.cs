using System.Reflection;
using System.Threading.Tasks;
using Fourth.Orchestration;
using Fourth.Orchestration.Messaging;
using Fourth.Orchestration.Model.Recipes;
using log4net;

namespace SampleEventListener
{
    /// <summary>
    /// Message handler that prints out the message ID and returns Success.
    /// </summary>
    public class RecipeUpdatedEventHandler : IMessageHandler<Events.RecipeUpdated>
    {
        /// <summary> The log4net Logger instance. </summary>
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public Task<MessageHandlerResult> HandleAsync(Events.RecipeUpdated payload, string trackingId)
        {
            Logger.Info("Listening to event in the handler: " + payload);

            // Return Success to indicate that we have finished processing
            return AsyncTask.FromResult(MessageHandlerResult.Success);
        }
    }
}