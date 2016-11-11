using Messaging.MSMQ;
using Messaging.MSMQ.Interface;
using System;
using System.Collections.Concurrent;
using System.Data;

namespace DataExported
{
    public class MessageBuilder
    {
        private static UpdateMessage MenuMessage()
        {
            return new UpdateMessage
            {

            };
        }
        private static UpdateMessage GroupMessage(DataTable dataTable)
        {
            return new UpdateMessage();
        }


        public static IMessage GetMessage(string entityName, ConcurrentDictionary<EntityEnum, DataTable> data)
        {
            IMessage output = null;

            if(!string.IsNullOrEmpty(entityName))
            {
                EntityEnum entityEnum;
                if (Enum.TryParse(entityName, out entityEnum))
                {
                    var record = data[entityEnum];

                    switch (entityEnum)
                    {
                        case EntityEnum.Menu:
                            output = MenuMessage();
                            break;
                        case EntityEnum.Group:
                            output = GroupMessage(record);
                            break;
                    }
                }
            }

            return output;
        }
    }
}
