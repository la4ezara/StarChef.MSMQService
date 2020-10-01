namespace StarChef.BackgroundServices.Common.Jobs
{
    public interface IBackgroundJob
    {
        void Execute(int databaseId);
    }
}
