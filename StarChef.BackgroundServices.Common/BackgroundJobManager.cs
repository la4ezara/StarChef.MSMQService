using Hangfire;
using Hangfire.Common;
using Hangfire.Storage;
using StarChef.BackgroundServices.Common.Jobs;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace StarChef.BackgroundServices.Common
{
    public class BackgroundJobManager
    {
        /// <summary>
        /// Creates a new fire-and-forget job
        /// </summary>
        public void Enqueue<T>(int databaseId) where T : IBackgroundJob
        {
            BackgroundJob.Enqueue<T>(x => x.Execute(databaseId));
        }

        /// <summary>
        /// Creates a new fire-and-forget job
        /// </summary>
        public void EnqueueCustom<T>(Expression<Action<T>> methodCall)
        {
            BackgroundJob.Enqueue<T>(methodCall);
        }

        /// <summary>
        /// Creates a new fire-and-forget job and schedules it to be enqueued after a given delay
        /// </summary>
        public void Schedule<T>(int databaseId, TimeSpan delay) where T : IBackgroundJob
        {
            BackgroundJob.Schedule<T>(x => x.Execute(databaseId), delay);
        }

        /// <summary>
        /// Creates a new fire-and-forget job and schedules it to be enqueued at the given moment of time
        /// </summary>
        public void Schedule<T>(int databaseId, DateTimeOffset enqueueAt) where T : IBackgroundJob
        {
            BackgroundJob.Schedule<T>(x => x.Execute(databaseId), enqueueAt);
        }

        /// <summary>
        /// AddsOrUpdates a new recurring job
        /// </summary>
        public void ScheduleRecurring<T>(int databaseId, string cronExpression, JobQueue queue = JobQueue.Default) where T : IBackgroundJob
        {
            var job = Job.FromExpression<T>(x => x.Execute(databaseId));
            var recurringJobId = GetRecurringJobId(job) + "_" + databaseId;

            RecurringJob.AddOrUpdate<T>(recurringJobId, x => x.Execute(databaseId), cronExpression, null, queue.ToString().ToLowerInvariant());
        }

        /// <summary>
        /// Deletes a recurring job
        /// </summary>
        public void Delete<T>(int databaseId) where T : IBackgroundJob
        {
            var job = Job.FromExpression<T>(x => x.Execute(databaseId));
            var recurringJobId = GetRecurringJobId(job) + "_" + databaseId;

            RecurringJob.RemoveIfExists(recurringJobId);
        }

        /// <summary>
        /// Check is recurring job existing
        /// </summary>
        public bool IsRecurringJobExists<T>(int databaseId) where T : IBackgroundJob
        {
            var job = Job.FromExpression<T>(x => x.Execute(databaseId));
            var recurringJobId = GetRecurringJobId(job) + "_" + databaseId;

            var reccuringJobs = Hangfire.JobStorage.Current.GetConnection().GetRecurringJobs()
                                                    .Where(x => x.Id.Equals(recurringJobId));

            if (reccuringJobs.Count() == 1)
            {
                return true;
            }

            return false;
        }

        public string GetJobId<T>(int databaseId) where T : IBackgroundJob
        {
            var job = Job.FromExpression<T>(x => x.Execute(databaseId));

            var recurringJobId = GetRecurringJobId(job) + "_" + databaseId;

            return recurringJobId;
        }

        public RecurringJobDto GetRecurringJob<T>(int databaseId) where T : IBackgroundJob
        {
            var job = Job.FromExpression<T>(x => x.Execute(databaseId));
            var recurringJobId = GetRecurringJobId(job) + "_" + databaseId;

            var reccuringJob = Hangfire.JobStorage.Current.GetConnection().GetRecurringJobs()
                                                    .FirstOrDefault(x => x.Id.Equals(recurringJobId));

            return reccuringJob;
        }

        private string GetRecurringJobId(Job job)
        {
            return job.Type.ToGenericTypeString() + "." + job.Method.Name;
        }
    }
}
