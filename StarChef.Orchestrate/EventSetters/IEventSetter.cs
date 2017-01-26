namespace StarChef.Orchestrate
{
    public interface IEventSetter<T>
    {
        bool SetBuilder(T builder, string connectionString, int entityId, int databaseId);
    }
}