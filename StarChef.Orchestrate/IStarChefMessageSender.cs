using System;
using StarChef.Common;
using StarChef.MSMQService;

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
    }
}