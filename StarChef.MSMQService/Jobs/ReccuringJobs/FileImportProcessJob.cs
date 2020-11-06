using Fourth.Import.Process;
using Hangfire.Logging;
using StarChef.BackgroundServices.Common.Jobs;
using System;

namespace StarChef.MSMQService.Jobs.ReccuringJobs
{
    public class FileImportProcessJob : IFileImportJob
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly ImportFileService _importService = new ImportFileService();

        public void Process(int databaseId, string externalId, string filePath, int importId)
        {
            var msg = $"databaseId:{databaseId} externalId:{externalId} filePath:'{filePath}' importId:{importId}";
            
            try
            {
                Logger.Info($"Processing FileImportProcessJob {msg}");
                var result = _importService.ImportFileUploaded(filePath, "StarChefLogin");
                Logger.Info($"Processed FileImportProcessJob {msg}");
            }
            catch (Exception e) {
                Logger.FatalException(msg, e);
                throw e;
            }
        }
    }
}
