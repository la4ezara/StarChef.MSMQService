using System;
using Google.ProtocolBuffers;
using StarChef.Common;
using StarChef.Data;
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
            int databaseId,
            DateTime messageArrivedTime);

        bool PublishDeleteEvent(UpdateMessage message);
        bool PublishCommand(UpdateMessage message);
    }
}