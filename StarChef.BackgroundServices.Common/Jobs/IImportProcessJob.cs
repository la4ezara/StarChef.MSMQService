using StarChef.BackgroundServices.Common.Attributes;

namespace StarChef.BackgroundServices.Common.Jobs
{
    [DefaultJobLogging]
    [CustomDisableConcurrentExecution(timeoutInSeconds: 300)]
    public interface IImportJob
    {
        void Process(int databaseId, int importId);
    }
}
