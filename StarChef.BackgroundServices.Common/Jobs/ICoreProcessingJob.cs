using StarChef.BackgroundServices.Common.Attributes;

namespace StarChef.BackgroundServices.Common.Jobs
{
    [DefaultJobLogging]
    [CustomDisableConcurrentExecution(timeoutInSeconds: 7200)]
    public interface ICoreProcessingJob : IBackgroundJob
    {
    }
}
