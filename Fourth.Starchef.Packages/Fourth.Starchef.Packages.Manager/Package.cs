﻿#region usings

using System;
using System.Configuration;
using System.IO;
using System.Runtime.Versioning;
using Fourth.Starchef.Packages.DataService;
using Fourth.Starchef.Packages.Model;
using Fourth.Starchef.Packages.Pdf;
using Fourth.Starchef.Packages.Report.Properties;
using Fourth.Starchef.Packages.Toc;
using Fourth.Starchef.Util;
using System.Resources;
#endregion

namespace Fourth.Starchef.Packages.Manager
{
    public class Package
    {
     public void Create(Config config, int packageId)
        {
            int logId=0;
            PackageService packageService = new PackageService
            {
                Config = config,
                PackageId = packageId
            };
            try
            {
                string convertedFolderPath = config.DocumentsConvertedFolder;

                logId = packageService.ProcessStartedLog("Package generation started", config.UserId);

                Model.Package package = packageService
                    .Load()
                    .WithSections()
                    .WithDocumentItems()
                    .WithReportItems()
                    .Package;

                package.LogId = logId;
                package.LogProgress = packageService.UpdateProcessLog;

                new AuxDocumentManager().Process(config, package, convertedFolderPath);
                new ReportsManager().Process(config, package);
                new PackageManager().Process(config, package, convertedFolderPath);

                string packagedFile;

                if (package.IncludeToc)
                {
                    package.LogProgress(package.LogId, "Started creating table of contents");
                    string tocFilePath = new TocManager().Create(package, config);
                    package.LogProgress(package.LogId, "Completed creating table of contents");
                    packagedFile = new Files().MergeDocs(config, package, tocFilePath);
                }
                else
                {
                    packagedFile = new Files().MergeDocs(config, package);
                }

                DocumentService documentService = new DocumentService();
                string description = string.Format("Package output for {0} generated by {1} {2} on {3}", package.Name, config.UserFirstName, config.UserLastName, DateTime.Now.ToString("dd MMM yyyy HH:mm:ss"));

                FileInfo fileInfo = new FileInfo(config.DocumentsConvertedFolder + packagedFile);
                double fileSize = fileInfo.Length/1000000.0;

                documentService.AddPackgeToDocuments(config, package.Name, description, packagedFile, fileSize.ToString("N2"),logId,package.Id);
        
                SendSuccessEmail(config);

            }
            catch (Exception e)
            {
                packageService.ProcessFailedLog(logId,"Package generation failed", e.ToString());
                SendFailureMail(config, packageId, logId, e);
            }
        }

        private static void SendSuccessEmail(Config config)
        {
            config.PackageCompletedMailSetting.ToEmail.Clear();
            config.PackageCompletedMailSetting.ToEmail.Add(config.UserEmailAddress);
            MailService.Mail(config.PackageCompletedMailSetting);
        }

        private static void SendFailureMail(Config config, int packageId, int logId, Exception e)
        {
            SendFailureMessageToUser(config);
            SendFailureMessageToAdmin(config, packageId,logId,e);
        }

        private static void SendFailureMessageToAdmin(Config config, int packageId, int logId, Exception e)
        {
            config.PackageFailedMailSetting.ToEmail.Clear();
            var bodyHeader = config.PackageFailedMailSetting.Body;
            config.PackageFailedMailSetting.ToEmail.Add(ConfigurationManager.AppSettings["exceptionToAddress"]);
            config.PackageFailedMailSetting.Body += string.Format("LogId:{0},PackageId:{1},<br/>{2}<br/>{3}", logId, packageId, e.Message, e.StackTrace);

            MailService.Mail(config.PackageFailedMailSetting);
            config.PackageFailedMailSetting.Body = bodyHeader;
        }
        
        private static void SendFailureMessageToUser(Config config)
        {
            config.PackageFailedMailSetting.ToEmail.Clear();
            var bodyHeader = config.PackageFailedMailSetting.Body;
            config.PackageFailedMailSetting.ToEmail.Add(config.UserEmailAddress);
            config.PackageFailedMailSetting.Body += Utils.GetAccountAdministrationMessage(config);

            MailService.Mail(config.PackageFailedMailSetting);
            config.PackageFailedMailSetting.Body = bodyHeader;
        }
    }
}