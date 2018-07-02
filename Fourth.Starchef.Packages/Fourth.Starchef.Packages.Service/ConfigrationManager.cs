#region usings

using System;
using System.Configuration;
using Fourth.Starchef.Packages.Model;

#endregion

namespace Fourth.Starchef.Packages.Service
{
    public class ConfigrationManager  /**/
    {
        public Config Load()
        {
            Config config = new Config
            {
                PackageQueuePath = ConfigurationManager.AppSettings["packageQueuePath"],
                PollIntervalMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["pollIntervalMinutes"]),
                EvoPdfLicenseKey = ConfigurationManager.AppSettings["evoPdfLicenseKey"],
                AsposeLicenseFile = ConfigurationManager.AppSettings["asposeLicenseFile"],
                LoginConnString = ConfigurationManager.ConnectionStrings["loginConnString"].ToString(),
                ReportingEngine1Url = ConfigurationManager.AppSettings["reportingEngine1Url"],
                ReportingEngine2Url = ConfigurationManager.AppSettings["reportingEngine2Url"],
                ReportOutputPath = ConfigurationManager.AppSettings["reportOutputPath"],
                ConvertExcelToOnePagePerSheet =
                    Convert.ToBoolean(ConfigurationManager.AppSettings["convertExcelToOnePagePerSheet"]),
                ImageBaseUrl = ConfigurationManager.AppSettings["imageBaseUrl"],
                HeaderFooterLogoResizedWidth =
                    Convert.ToDouble(ConfigurationManager.AppSettings["headerFooterLogoResizedWidth"])
            };

            config.HeaderFooterConfig = SetHeaderFooterConfig();
            config.PackageFailedMailSetting = SetFailedMailSettings();
            config.PackageCompletedMailSetting = SetSuccessMailSettings();

            return config;
        }

        private MailSetting SetFailedMailSettings()
        {
            var packageFailed = new MailSetting
            {
                FromEmail = ConfigurationManager.AppSettings["fromAddress"],
                Subject = ConfigurationManager.AppSettings["exceptionSubject"],
                Body = ConfigurationManager.AppSettings["exceptionBody"],
                IsHighImportance = true
            };

            return packageFailed;
        }

        private MailSetting SetSuccessMailSettings()
        {
            var packageCompleted = new MailSetting
            {
                FromEmail = ConfigurationManager.AppSettings["fromAddress"],
                Subject = ConfigurationManager.AppSettings["successSubject"],
                Body = ConfigurationManager.AppSettings["SuccessBody"]
            };

            return packageCompleted;
        }

        private HeaderFooterConfig SetHeaderFooterConfig()
        {
            var headerFooterConfig = new HeaderFooterConfig
            {
                HeaderMargin =
                {
                    Top = Convert.ToDouble(ConfigurationManager.AppSettings["headerTopMargin"]),
                    Bottom = Convert.ToDouble(ConfigurationManager.AppSettings["headerBottomMargin"]),
                    Left = Convert.ToDouble(ConfigurationManager.AppSettings["headerLeftMargin"]),
                    Right = Convert.ToDouble(ConfigurationManager.AppSettings["headerRightMargin"])
                },
                FooterMargin =
                {
                    Top = Convert.ToDouble(ConfigurationManager.AppSettings["footerTopMargin"]),
                    Bottom = Convert.ToDouble(ConfigurationManager.AppSettings["footerBottomMargin"]),
                    Left = Convert.ToDouble(ConfigurationManager.AppSettings["footerLeftMargin"]),
                    Right = Convert.ToDouble(ConfigurationManager.AppSettings["footerRightMargin"])
                },
                MarginTopOffset = Convert.ToDouble(ConfigurationManager.AppSettings["marginTopOffset"]),
                MarginBottomOffset = Convert.ToDouble(ConfigurationManager.AppSettings["marginBottomOffset"])
            };

            return headerFooterConfig;
        }
    }
}