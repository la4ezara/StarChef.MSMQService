using System;
using System.Collections.Generic;
using StarChef.Common;
using UpdateMessage = StarChef.MSMQService.UpdateMessage;

namespace StarChef.Orchestrate
{
    public interface IStarChefMessageSender
    {
        bool Send(
            EnumHelper.EntityTypeWrapper entityTypeWrapper, 
            string dbConnectionString,
            int entityTypeId,
            int entityId,
            string entityExternalId,
            int databaseId,
            DateTime messageArrivedTime);

        bool Send(
            EnumHelper.EntityTypeWrapper entityTypeWrapper,
            string dbConnectionString,
            int entityTypeId,
            List<int> entityIds,
            string entityExternalId,
            int databaseId,
            DateTime messageArrivedTime
        );

        bool PublishDeleteEvent(UpdateMessage message);
        bool PublishUpdateEvent(UpdateMessage message);
        bool PublishCommand(UpdateMessage message);
    }
}