using Messaging.MSMQ.Interface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using Messaging.MSMQ;
using static Messaging.MSMQ.Constants;

namespace DataExported
{
    public class MessageBuilder
    {
        private string dbDSN;
        private int databaseId;
        public MessageBuilder()
        {

        }
        public MessageBuilder(string dsn, int databaseId)
        {
            this.dbDSN = dsn;
            this.databaseId = databaseId;
        }

        private IList<IMessage> CreateMessage(Constants.EntityType entityType, DataTable dataTable)
        {
            IList<IMessage> messages = new List<IMessage>();

            foreach (var row in dataTable.Rows)
            {
                var entityId = Convert.ToInt32(((DataRow)row)[0]);

                messages.Add(new UpdateMessage(entityId,
                                            this.dbDSN,
                                            (int)Constants.MessageActionType.StarChefEventsUpdated,
                                            this.databaseId,
                                            (int)entityType));

            }

            return messages;
        }

        public IEnumerable<IMessage> GetMessages(
            EntityEnum entity, 
            ConcurrentDictionary<EntityEnum, DataTable> data
            )
        {
            IList<IMessage> output = null;

            if(data != null)
            {
                var record = data[entity];

                switch (entity)
                {
                    case EntityEnum.Menu:
                        output = CreateMessage(EntityType.Menu, record);
                        break;
                    case EntityEnum.Group:
                        output = CreateMessage(EntityType.Group, record);
                        break;
                    case EntityEnum.Recipe:
                        output = CreateMessage(EntityType.Dish, record);
                        break;
                    case EntityEnum.User:
                        output = CreateMessage(EntityType.User, record);
                        break;
                    case EntityEnum.MealPeriod:
                        output = CreateMessage(EntityType.MealPeriodManagement, record);
                        break;
                }
            }

            return output;
        }
    }
}