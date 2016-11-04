﻿using System;
using System.Configuration;
using System.Data.Common;
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
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public PriceBandEventHandler()
        {
        }

        public PriceBandEventHandler(IDatabaseCommands customerDb) : base(customerDb)
        {
        }

        public async Task<MessageHandlerResult> HandleAsync(Events.PriceBandUpdated priceBandUpdated, string trackingId)
        {
            #region validation

            if (!priceBandUpdated.HasCustomerId)
            {
                Logger.Error(string.Format("Price Band message, with tracking id {0}, received with no customer Id", trackingId));
                return MessageHandlerResult.Fatal;
            }

            if (priceBandUpdated.PriceBandsCount == 0)
            {
                Logger.Error(string.Format("Price Band message, with tracking id: {0}, received for customer: {1}", trackingId, priceBandUpdated.CustomerId));
                return MessageHandlerResult.Fatal;
            }

            #endregion

            var organisationGuid = new Guid(priceBandUpdated.CustomerId);

            try
            {
                Logger.Info("Start message processing");

                var xmlDoc = priceBandUpdated.ToXml();
                if (xmlDoc == null)
                {
                    Logger.Info(string.Format("There is no valid price band to process for the message, tracking id: {0}, for customer {1}", trackingId, organisationGuid));
                    return MessageHandlerResult.Success;
                }
                await DbCommands.SaveData(organisationGuid, xmlDoc);
                Logger.Info(string.Format("Successfully updated price band details: customer id: {0}, tracking id: {1}", organisationGuid, trackingId));
                return MessageHandlerResult.Success;
            }
            catch (CustomerDbNotFoundException) {
                return MessageHandlerResult.Success;
            }
            catch (LoginDbNotFoundException ex)
            {
                Logger.Error("Error getting login database", ex);
                return MessageHandlerResult.Fatal;
            }
            catch (DataNotSavedException ex)
            {
                Logger.Error(string.Format("Price band update failed: customer id: {0}, tracking id: {1}", organisationGuid, trackingId), ex);
                return MessageHandlerResult.Fatal;
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Failed to handle the event \"{0}\" [Customer Guid: {1}].", priceBandUpdated.GetType().Name, organisationGuid), ex);
                return MessageHandlerResult.Retry;
            }
        }
    }
}