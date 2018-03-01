namespace StarChef.SqlQueue.Service.Interface
{
    public interface IAppConfiguration
    {
        string UserDSN { get; }
        int MessagesCount { get; }
        int RetryCount { get; }
        int SleepMinutes { get; }
        int NewThreadMessages { get; }
        int MaxThreadCount { get; }
    }
}
