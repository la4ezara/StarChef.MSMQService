using System.Messaging;

namespace Fourth.Import.Common.Messaging
{
    public interface IMessagingSettings
    {
        MessagePriority GetMessagePriority();
        string GetQueueName();
    }
}