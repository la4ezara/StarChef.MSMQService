using StarChef.BackgroundServices.Common.Attributes;

namespace StarChef.BackgroundServices.Common.Jobs
{
    [DefaultJobLogging]
    [CustomDisableConcurrentExecution(timeoutInSeconds: 3600)]
    public interface ICoreProcessingJob : IBackgroundJob
    {
    }
}
