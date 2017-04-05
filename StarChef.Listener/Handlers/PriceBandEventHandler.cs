using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Fourth.Orchestration.Messaging;
using log4net;
using StarChef.Listener.Commands;
using StarChef.Listener.Extensions;

using PriceBandUpdated = Fourth.Orchestration.Model.Recipes.Events.PriceBandUpdated;

namespace StarChef.Listener.Handlers
{
    public class PriceBandEventHandler : ListenerEventHandler, IMessageHandler<PriceBandUpdated>
    {
        private readonly IConfiguration _configuration;
        private readonly ILog _logger;

        public PriceBandEventHandler(IDatabaseCommands dbCommands, IEventValidator validator, IMessagingLogger messagingLogger, IConfiguration configuration) : base(dbCommands, validator, messagingLogger)
        {
            _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
            _configuration = configuration;
        }
        public PriceBandEventHandler(IDatabaseCommands dbCommands, IEventValidator validator, IMessagingLogger messagingLogger, IConfiguration configuration, ILog errorLogger) : base(dbCommands, validator, messagingLogger)
        {
            _logger = errorLogger;
            _configuration = configuration;
        }

        public async Task<MessageHandlerResult> HandleAsync(PriceBandUpdated priceBandUpdated, string trackingId)
        {
            ThreadContext.Properties[DATABASE_GUID] = priceBandUpdated.CustomerId;

            // whole payload cannot be logged because the list may be very big
            _logger.InfoFormat("Data received. CustomerId={0}, CurrencyId={1}, PriceBandsCount={2}", priceBandUpdated.CustomerId, priceBandUpdated.CurrencyId, priceBandUpdated.PriceBandsCount);

            try
            {
                if (!Validator.IsEnabled(priceBandUpdated))
                {
                    _logger.EventDisabledForOrganization(priceBandUpdated);
                    return MessageHandlerResult.Success;
                }
                var priceBandBatchSize = _configuration.PriceBandBatchSize;

                if (Validator.IsValidPayload(priceBandUpdated))
                {
                    var organisationGuid = new Guid(priceBandUpdated.CustomerId);

                    try
                    {
                        if (!priceBandUpdated.PriceBandsList.Any())
                        {
                            _logger.InfoFormat("Processed");

                            return MessageHandlerResult.Success;
                        }

                        var blocks = Convert.ToInt32(priceBandUpdated.PriceBandsCount / priceBandBatchSize);
                        var tailBlockSize = Convert.ToInt32(priceBandUpdated.PriceBandsCount % priceBandBatchSize);
                        if (tailBlockSize > 0) blocks += 1;

                        _logger.InfoFormat("Processing {0} blocks of data, each {1} rows, the last block contains {2} rows",
                            blocks,
                            priceBandBatchSize,
                            tailBlockSize);

                        var blockNum = 0;
                        foreach (var xml in priceBandUpdated.ToSmallXmls(priceBandBatchSize))
                        {
                            _logger.InfoFormat("Processing: " + xml.InnerXml);
                            await DbCommands.SavePriceBandData(organisationGuid, xml);
                            _logger.InfoFormat("Processed blocks {0} of {1}", ++blockNum, blocks);
                        }

                        _logger.InfoFormat("Processed");

                        return MessageHandlerResult.Success;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex.Message, ex);
                        return MessageHandlerResult.Retry;
                    }
                }

                var errors = Validator.GetErrors();
                _logger.InvalidModel(trackingId, priceBandUpdated, errors);
                await MessagingLogger.ReceivedInvalidModel(trackingId, priceBandUpdated, errors);
            }
            finally
            {
                ThreadContext.Properties.Remove(DATABASE_GUID);
            }
            
            return MessageHandlerResult.Fatal;
        }
    }
}