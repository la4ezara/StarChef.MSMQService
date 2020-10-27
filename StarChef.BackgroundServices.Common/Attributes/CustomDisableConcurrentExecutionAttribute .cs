using Hangfire.Common;
using Hangfire.Server;
using log4net;
using System;
using System.Reflection;

namespace StarChef.BackgroundServices.Common.Attributes
{
    /// <summary>
    /// Custom implementation of the <see cref="DisableConcurrentExecutionAttribute"/> that takes into account the parameters for job ID
    /// </summary>
    public class CustomDisableConcurrentExecutionAttribute : JobFilterAttribute, IServerFilter
    {
        protected ILog Logger => LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly int _timeoutInSeconds;

        public CustomDisableConcurrentExecutionAttribute(int timeoutInSeconds)
        {
            if (timeoutInSeconds < 0)
            {
                throw new ArgumentException("Timeout argument value should be greater than zero.");
            }

            _timeoutInSeconds = timeoutInSeconds;
        }

        public void OnPerforming(PerformingContext filterContext)
        {
            var resource = GetResource(filterContext.BackgroundJob.Job);
            var timeout = TimeSpan.FromSeconds(_timeoutInSeconds);

            try
            {
                var disposable = filterContext.Connection.AcquireDistributedLock(resource, timeout);
                filterContext.Items["DistributedLock"] = disposable;
            }
            catch (Exception ex)
            {
                Logger.Warn("HangFire couldn't acquire distributed lock", ex);
                filterContext.Canceled = true;
            }
        }

        public void OnPerformed(PerformedContext filterContext)
        {
            if (!filterContext.Items.ContainsKey("DistributedLock"))
            {
                string message = "HangFire can't release a distributed lock: it was not acquired.";
                Logger.Warn(message);
                throw new InvalidOperationException(message);
            }

            var distributedLock = (IDisposable)filterContext.Items["DistributedLock"];
            distributedLock.Dispose();
        }

        private static string GetResource(Job job)
        {
            return job.Type.ToGenericTypeString() + "." + job.Method.Name + "_" + string.Join("_", job.Args);
        }
    }
}
