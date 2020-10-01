using Hangfire.Common;
using Hangfire.Logging;
using Hangfire.States;
using Hangfire.Storage;

namespace StarChef.BackgroundServices.Common.Attributes
{
    public class DefaultJobLoggingAttribute : JobFilterAttribute, IApplyStateFilter
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            var failedState = context.NewState as FailedState;
            if (failedState != null)
            {
                Logger.ErrorException(
                    $"Background job #{context.BackgroundJob.Id} has failed with an exception.",
                    failedState.Exception
                );
            }
        }

        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            //Do nothing for the moment
        }
    }
}
