using StarChef.BackgroundServices.Common.Attributes;

namespace StarChef.BackgroundServices.Common.Jobs
{
    [DefaultJobLogging]
    //[CustomDisableConcurrentExecution(timeoutInSeconds: 120)]
    public interface ICoreProcessingJob : IBackgroundJob
    {
    }
}
