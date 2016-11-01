using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Fourth.Orchestration.Messaging;
using Fourth.Orchestration.Model.Recipes;
using log4net;

namespace StarChef.Listener
{
    public class PriceBandEventHandler : IMessageHandler<Events.PriceBandUpdated>
    {
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public async Task<MessageHandlerResult> HandleAsync(Events.PriceBandUpdated priceBandUpdated, string trackingId)
        {
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

            var organisationGuid = new Guid(priceBandUpdated.CustomerId);

            var transactionConnectionString = string.Empty;

            try
            {
                Logger.Info("Start message processing");
                try
                {
                    transactionConnectionString = await GetCustomerDbConnectionString(organisationGuid);
                }
                catch (Exception ex)
                {
                    Logger.Error("Error getting database of organisation", ex);
                    return MessageHandlerResult.Fatal;
                }

                var xmlString = new StringBuilder();

                foreach (var priceBand in priceBandUpdated.PriceBandsList)
                {
                    if (!priceBand.HasId || (!priceBand.HasMinimumPrice && !priceBand.HasMaximumPrice))
                        continue;

                    xmlString.Append($"<PriceBand><ProductGuid>{priceBand.Id}</ProductGuid><MinPrice>{priceBand.MinimumPrice}</MinPrice><MaxPrice>{priceBand.MaximumPrice}</MaxPrice></PriceBand>");
                }

                if (string.IsNullOrEmpty(xmlString.ToString()))
                {
                    Logger.Info($"There is no valid price band to process for the message, tracking id: {trackingId}, for customer {organisationGuid}");
                    return MessageHandlerResult.Success;
                }

                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml($"<PriceBandList>{xmlString}</PriceBandList>");

                if (!string.IsNullOrEmpty(transactionConnectionString))
                {
                    using (var sqlConn = new SqlConnection(transactionConnectionString))
                    {
                        await sqlConn.OpenAsync();

                        using (var sqlCmd = new SqlCommand("sc_save_product_price_band_list", sqlConn))
                        {
                            sqlCmd.CommandType = CommandType.StoredProcedure;
                            sqlCmd.Parameters.Add("@PriceBandListXml", SqlDbType.Xml).Value = xmlDoc.InnerXml;

                            try
                            {
                                await sqlCmd.ExecuteNonQueryAsync();

                                Logger.Info($"Successfully updated price band details: customer id: {organisationGuid}, tracking id: {trackingId}");
                            }
                            catch (Exception ex)
                            {
                                Logger.Error($"Price band update failed: customer id: {organisationGuid}, tracking id: {trackingId}", ex);
                                return MessageHandlerResult.Fatal;
                            }
                        }
                    }
                }

                return MessageHandlerResult.Success;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to handle the event \"{priceBandUpdated.GetType().Name}\" [Customer Guid: {organisationGuid}].", ex);
                return MessageHandlerResult.Retry;
            }
        }

        private async Task<string> GetCustomerDbConnectionString(Guid organisationGuid)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["StarchefLogin"];

            using (var sqlConnection = new SqlConnection(connectionString.ConnectionString))
            {
                await sqlConnection.OpenAsync();

                using (var sqlCmd = new SqlCommand("sc_database_GetByOrgGuid", sqlConnection))
                {
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.Parameters.Add("@OrganisationGuid", SqlDbType.UniqueIdentifier).Value = organisationGuid;

                    var rtnVal = sqlCmd.ExecuteScalar();

                    if (rtnVal == DBNull.Value)
                    {
                        Logger.Error($"There is no organisation with the given organisation Guid: {organisationGuid}");
                        return null;
                    }
                    return rtnVal.ToString();
                }
            }
        }
    }
}
