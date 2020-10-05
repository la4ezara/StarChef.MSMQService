﻿using Hangfire.Logging;
using StarChef.BackgroundServices.Common.Jobs;
using System;
using System.Configuration;
using System.Text;
using StarChef.Common;
using StarChef.MSMQService.Configuration;
using Fourth.StarChef.Invariables;
using Fourth.StarChef.Invariables.Interfaces;
using log4net;
//using ILog = Hangfire.Logging.ILog;

namespace StarChef.MSMQService.Jobs.ReccuringJobs
{
    public class CoreProcessingJob : ICoreProcessingJob
    {
        //protected static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        private static readonly log4net.ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //private readonly IAppConfiguration _appConfiguration;
        private readonly IDatabaseManager _databaseManager;
        private readonly IOrganizationManager _orgManager;

        public CoreProcessingJob(IAppConfiguration appConfiguration, IDatabaseManager databaseManager)
        {
            //_appConfiguration = appConfiguration;
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

                var orgConnectionString = _orgManager.GetConnectionById(databaseId);
                if (!string.IsNullOrEmpty(orgConnectionString))
                {
                    IBackgroundTaskManager taskManager = new BackgroundTaskManager(orgConnectionString);
                    //var tasks = taskManager.ListTasks(null, null, null, 100, 1);
                    //BackgroundTaskProcessor processor = new BackgroundTaskProcessor(databaseId, org.ConnectionString, _databaseManager, Logger);

                    //foreach(var t in tasks)
                    //{
                    //    try
                    //    {
                    //        Logger.Info($"Task {t.Id} Processing");
                    //        var res = taskManager.UpdateTaskStatus(t.Id, t.Status, Fourth.StarChef.Invariables.Enums.BackgroundTaskStatus.InProgress, string.Empty).Result;
                    //        processor.ProcessMessage(t);
                    //        res = taskManager.UpdateTaskStatus(t.Id, t.Status, Fourth.StarChef.Invariables.Enums.BackgroundTaskStatus.Completed, string.Empty).Result;
                    //        Logger.Info($"Task {t.Id} Processing Complete");
                    //    }
                    //    catch(Exception ex)
                    //    {
                    //        var res = taskManager.UpdateTaskStatus(t.Id, t.Status, Fourth.StarChef.Invariables.Enums.BackgroundTaskStatus.Failed, ex.Message).Result;

                    //    }
                    //}
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
