using Google.ProtocolBuffers;

namespace StarChef.Orchestrate
{
    public interface IEventFactory
    {
        TMessage CreateUpdateEvent<TMessage, TBuilder>(string connectionString, int entityId, int databaseId)
            where TMessage : GeneratedMessage<TMessage, TBuilder>
            where TBuilder : GeneratedBuilder<TMessage, TBuilder>, new();

        TMessage CreateDeleteEvent<TMessage, TBuilder>(string connectionString, string entityExternalId, int databaseId)
            where TMessage : GeneratedMessage<TMessage, TBuilder>
            where TBuilder : GeneratedBuilder<TMessage, TBuilder>, new();
    }
}