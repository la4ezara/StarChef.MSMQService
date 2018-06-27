using System;
using System.Collections.Generic;
using Fourth.StarChef.Invariables;
using StarChef.Common;

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

        IList<KeyValuePair<Tuple<int, int>, bool>> Send(
            EnumHelper.EntityTypeWrapper entityTypeWrapper,
            string dbConnectionString,
            int entityTypeId,
            List<int> entityIds,
            string entityExternalId,
            int databaseId,
            DateTime messageArrivedTime
        );

        bool PublishDeleteEvent(UpdateMessage message);
        bool PublishCommand(UpdateMessage message);
    }
}