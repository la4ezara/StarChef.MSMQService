﻿using Fourth.StarChef.Invariables;
using Fourth.StarChef.Invariables.Interfaces;
using log4net;
using StarChef.BackgroundServices.Common.Jobs;
using StarChef.Common;
using StarChef.Common.Engine;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Text;

namespace StarChef.MSMQService.Jobs.ReccuringJobs
{
    public class CoreProcessingJob : ICoreProcessingJob
    {
        private static readonly log4net.ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IDatabaseManager _databaseManager;
        private readonly IOrganizationManager _orgManager;

        public CoreProcessingJob(IDatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
            _orgManager = new OrganizationManager(ConfigurationManager.ConnectionStrings["SL_login"].ConnectionString);
        }


        public void Execute(int databaseId)
        {
            try
            {
                Logger.Info($"{GetType().Name} - [Common Background Tasks] Starting job for organization with database id: {databaseId}");
                //should get older background tasks with status new and one by one to try processing it before processing try to change status and after processing update status
                //processing itself should happen like listener.Process method 

                var org = _orgManager.GetById(databaseId);
                if (!string.IsNullOrEmpty(org.ConnectionString))
                {
                    IBackgroundTaskManager taskManager = new BackgroundTaskManager(org.ConnectionString, Process.GetCurrentProcess().ProcessName);

                    var repo = new Common.Repository.PricingRepository(org.ConnectionString, Constants.TIMEOUT_MSMQ_EXEC_STOREDPROC);
                    var engine = new PriceEngine(repo, Logger);

                    var tasks = taskManager.ListTasks(Fourth.StarChef.Invariables.Enums.BackgroundTaskStatus.New, null, null, false, 100, 0);
                    BackgroundTaskProcessor processor = new BackgroundTaskProcessor(databaseId, org.ConnectionString, _databaseManager, engine, Logger);

                    foreach (var t in tasks)
                    {
                        try
                        {
                            Logger.Info($"Task {t.Id} Processing");
                            var res = taskManager.UpdateTaskStatus(t.Id, t.Status, Fourth.StarChef.Invariables.Enums.BackgroundTaskStatus.InProgress, string.Empty).Result;
                            processor.ProcessMessage(t);
                            res = taskManager.UpdateTaskStatus(t.Id, Fourth.StarChef.Invariables.Enums.BackgroundTaskStatus.InProgress, Fourth.StarChef.Invariables.Enums.BackgroundTaskStatus.Completed, string.Empty).Result;
                            Logger.Info($"Task {t.Id} Processing Complete");
                        }
                        catch (Exception ex)
                        {
                            var res = taskManager.UpdateTaskStatus(t.Id, null, Fourth.StarChef.Invariables.Enums.BackgroundTaskStatus.Failed, ex.Message).Result;
                        }
                    }
                }
                else
                {
                    Logger.Warn($"OrganizationId {databaseId} not found");
                }

                Logger.Info($"{GetType().Name} - [Common Background Tasks] Finishing job for organization with database id: {databaseId}");
            }
            catch (Exception ex)
            {
                var errorMessage = new StringBuilder();
                errorMessage.AppendFormat($"{GetType().Name} - [Common Background Tasks] error for organization with database id: {databaseId} ");

                if (!string.IsNullOrEmpty(ex.Message))
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