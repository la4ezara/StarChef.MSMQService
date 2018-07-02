namespace Fourth.Import.Process
{
    using Fourth.Import.ExcelService;
    using Fourth.Import.Exceptions;
    using log4net;
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;

    //Note: Please refactor the whole class and remove event logging
    public class ImportFileService
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ImportFileService()
        {

        }

        private void CopySupplierImportFiles(string filePath)
        {
            int startHour = Convert.ToInt32(ConfigurationManager.AppSettings["SupplierImportStartTime"]);
            int endHour = Convert.ToInt32(ConfigurationManager.AppSettings["SupplierImportEndTime"]);
            string supplierImportFolder = ConfigurationManager.AppSettings["SupplierImportFilePath"];
            DateTime currentTime = DateTime.Now;
            TimeSpan startTime = new TimeSpan(startHour, 0, 0);
            TimeSpan endTime = new TimeSpan(endHour, 0, 0);
            if (currentTime.TimeOfDay > startTime && currentTime.TimeOfDay < endTime)
            {
                FileService fileService = new FileService();
                fileService.MoveFile(filePath, "");
            }
        }

        public void ImportFileUploaded(string filePath, string sourceLogin)
        {
            EventLog myLog = new EventLog();
            myLog.Source = "Fourth Import";
            myLog.WriteEntry("watcher triggered.");


            FileInfo file = new FileInfo(filePath);
            AddFileToQueue(file, sourceLogin);
        }

        private static void AddFileToQueue(FileInfo fileInfo, string sourceLogin)
        {

            try
            {
                while (true)
                {
                    if (!FileUploadComplete(fileInfo)) continue;
                    IngredientImport ingredientImport = new IngredientImport();
                    ingredientImport.Process(fileInfo, sourceLogin);
                    break;
                }
            }
            catch (Exception e)
            {
                _logger.Error("AddFileToQueue", e);

                //mute failed mail sending
                try
                {
                    MailSetting defaultMailsetting = new MailSetting
                    {
                        FromEmail = ConfigurationManager.AppSettings["fromAddress"],
                    };

                    defaultMailsetting.Subject = ConfigurationManager.AppSettings["subject"];
                    defaultMailsetting.ToEmail = ConfigurationManager.AppSettings["toAddress"].Split(',').ToList();
                    defaultMailsetting.Alias = ConfigurationManager.AppSettings["alias"];

                    ExceptionManager.Publish(e.Message, e.StackTrace, defaultMailsetting);
                }
                catch (Exception) { }
            }
        }

        private static bool FileUploadComplete(FileInfo file)
        {
            Thread.Sleep(3000);

            try
            {
                using (file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return true;
                }
            }
            catch (IOException)
            {
                return false;
            }
        }
    }
}