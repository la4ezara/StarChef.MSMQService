using StarChef.BackgroundServices.Common.Attributes;

namespace StarChef.BackgroundServices.Common.Jobs
{
    [DefaultJobLogging]
    [CustomDisableConcurrentExecution(timeoutInSeconds: 300)]
    public interface IFileImportJob
    {
        void Process(int databaseId, string externalId, string filePath, int importId);
    }
}
