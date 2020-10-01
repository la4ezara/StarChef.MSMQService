using Hangfire.Logging;
using StarChef.BackgroundServices.Common.Jobs;
using System;
using System.Text;

namespace StarChef.MSMQService.Jobs.ReccuringJobs
{
    public class CoreProcessingJob : ICoreProcessingJob
    {
        protected static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        public void Execute(int databaseId)
        {
            try
            {
                Logger.Info($"{GetType().Name} - [Common Background Tasks] Starting job for organisation with database id: {databaseId}");
                //TO DO: Implement the functionality for the recosting
                //should get older background tasks with status new and one by one to try procesing it before processing try to change status and after processing update status
                //processing itself should happen like listener.Process method 

                Logger.Info($"{GetType().Name} - [Common Background Tasks] Finishing job for organisation with database id: {databaseId}");
            }
            catch (Exception ex)
            {
                var errorMessage = new StringBuilder();
                errorMessage.AppendFormat($"{GetType().Name} - [Common Background Tasks] error for organisation with database id: {databaseId} ");

                if (ex != null && (!string.IsNullOrEmpty(ex.Message)))
                {
                    errorMessage.AppendFormat($"Exception message is: {ex.Message} ");
                }

                if (ex.InnerException != null && (!string.IsNullOrEmpty(ex.InnerException.Message)))
                {
                    errorMessage.AppendFormat($"Inner Exception message is: {ex.InnerException.Message}");
                }

                Logger.Error(errorMessage.ToString());
            }
        }
    }
}
