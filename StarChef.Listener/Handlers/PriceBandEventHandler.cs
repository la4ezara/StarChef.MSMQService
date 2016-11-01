using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Xml;
using Fourth.Orchestration.Messaging;
using Fourth.Orchestration.Model.Recipes;
using log4net;
using StarChef.Listener.Types;
using System.Configuration;

namespace StarChef.Listener.Handlers
{
    public class PriceBandEventHandler : IMessageHandler<Events.PriceBandUpdated>
    {
        private readonly ICustomerDb _customerDb;

        public PriceBandEventHandler()
            : this(new PriceBandCustomerDb(ConfigurationManager.ConnectionStrings["StarchefLogin"].ConnectionString))
        {

        }

        public PriceBandEventHandler(ICustomerDb customerDb)
        {
            _customerDb = customerDb;
        }

        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public async Task<MessageHandlerResult> HandleAsync(Events.PriceBandUpdated priceBandUpdated, string trackingId)
        {
            #region validation
            if (!priceBandUpdated.HasCustomerId)
            {
                Logger.Error($"Price Band message, with tracking id {trackingId}, received with no customer Id");
                return MessageHandlerResult.Fatal;
            }

            if (priceBandUpdated.PriceBandsCount == 0)
            {
                Logger.Error($"Price Band message, with tracking id: {trackingId}, received for customer: {priceBandUpdated.CustomerId}");
                return MessageHandlerResult.Fatal;
            }
            #endregion

            var organisationGuid = new Guid(priceBandUpdated.CustomerId);

            try
            {
                Logger.Info("Start message processing");

                string customerDbConnectionString;
                try
                {
                    customerDbConnectionString = await _customerDb.GetConnectionString(organisationGuid);
                    if (string.IsNullOrEmpty(customerDbConnectionString))
                        return MessageHandlerResult.Success;
                }
                catch (Exception ex)
                {
                    Logger.Error("Error getting database of organisation", ex);
                    return MessageHandlerResult.Fatal;
                }

                var xmlDoc = priceBandUpdated.ToXml();
                if (xmlDoc == null)
                {
                    Logger.Info($"There is no valid price band to process for the message, tracking id: {trackingId}, for customer {organisationGuid}");
                    return MessageHandlerResult.Success;
                }

                try
                {
                    await _customerDb.SaveDataToDb(customerDbConnectionString, xmlDoc);
                    Logger.Info($"Successfully updated price band details: customer id: {organisationGuid}, tracking id: {trackingId}");
                    return MessageHandlerResult.Success;
                }
                catch (Exception ex)
                {
                    Logger.Error($"Price band update failed: customer id: {organisationGuid}, tracking id: {trackingId}", ex);
                    return MessageHandlerResult.Fatal;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to handle the event \"{priceBandUpdated.GetType().Name}\" [Customer Guid: {organisationGuid}].", ex);
                return MessageHandlerResult.Retry;
            }
        }
    }
}