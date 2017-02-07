using System;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Fourth.Orchestration.Messaging;
using Fourth.Orchestration.Model.Recipes;
using log4net;
using StarChef.Listener.Commands;
using StarChef.Listener.Commands.Impl;
using StarChef.Listener.Exceptions;
using StarChef.Listener.Extensions;

namespace StarChef.Listener.Handlers
{
    public class PriceBandEventHandler : ListenerEventHandler, IMessageHandler<Events.PriceBandUpdated>
    {
        private IConfiguration _configuration;
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public PriceBandEventHandler(IDatabaseCommands dbCommands, IEventValidator validator, IMessagingLogger messagingLogger, IConfiguration configuration) : base(dbCommands, validator, messagingLogger)
        {
            _configuration = configuration;
        }

        public async Task<MessageHandlerResult> HandleAsync(Events.PriceBandUpdated priceBandUpdated, string trackingId)
        {
            var priceBandBatchSize = _configuration.PriceBandBatchSize;

            #region validation

            if (!priceBandUpdated.HasCustomerId)
            {
                _logger.Error(string.Format("Price Band message, with tracking id {0}, received with no customer Id", trackingId));
                return MessageHandlerResult.Fatal;
            }

            if (priceBandUpdated.PriceBandsCount == 0)
            {
                _logger.Error(string.Format("Price Band message, with tracking id: {0}, received for customer: {1}", trackingId, priceBandUpdated.CustomerId));
                return MessageHandlerResult.Fatal;
            }

            #endregion

            var organisationGuid = new Guid(priceBandUpdated.CustomerId);

            try
            {
                // whole payload cannot be logged because the list may be very big
                _logger.InfoFormat("Data received. CustomerId={0}, CurrencyId={1}, PriceBandsCount={2}", priceBandUpdated.CustomerId, priceBandUpdated.CurrencyId, priceBandUpdated.PriceBandsCount);

                if (!priceBandUpdated.PriceBandsList.Any())
                {
                    _logger.InfoFormat("Processed");
                    return MessageHandlerResult.Success;
                }

                var blocks = Convert.ToInt32(priceBandUpdated.PriceBandsCount/priceBandBatchSize);
                var tailBlockSize = Convert.ToInt32(priceBandUpdated.PriceBandsCount%priceBandBatchSize);
                if (tailBlockSize > 0) blocks += 1;

                _logger.InfoFormat("Processing {0} blocks of data, each {1} rows, the last block contains {2} rows",
                    blocks,
                    priceBandBatchSize,
                    tailBlockSize);

                var blockNum = 0;
                foreach (var xml in priceBandUpdated.ToSmallXmls(priceBandBatchSize))
                {
                    await DbCommands.SavePriceBandData(organisationGuid, xml);
                    _logger.InfoFormat("Processed blocks {0} of {1}", ++blockNum, blocks);
                }

                _logger.InfoFormat("Processed");
                return MessageHandlerResult.Success;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return MessageHandlerResult.Retry;
            }
        }
    }
}