namespace StarChef.Orchestrate
{
    public interface IEventSetter<T>
    {
        bool SetForUpdate(T builder, string connectionString, int entityId, int databaseId);
        bool SetForDelete(T builder, string entityExternalId, int databaseId);
    }
}