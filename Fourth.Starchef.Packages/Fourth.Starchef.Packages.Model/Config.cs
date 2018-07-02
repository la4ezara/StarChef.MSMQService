namespace Fourth.Starchef.Packages.Model
{
    public class Config
    {
        public string ConnString { get; set; }
        public string PackageQueuePath { get; set; }
        public int PollIntervalMinutes { get; set; }
        public string EvoPdfLicenseKey { get; set; }
        public string AsposeLicenseFile { get; set; }
        public string LoginConnString { get; set; }
        public string DocumentsSourceFolder { get; set; }

        public string DocumentsConvertedFolder
        {
            get { return DocumentsFolder; }
        }

        public string DocumentsFolder { get; set; }
        public int UserId { get; set; }
        public string UserEmailAddress { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string ReportingEngine1Url { get; set; }
        public string ReportingEngine2Url { get; set; }
        public string ReportOutputPath { get; set; }
        public bool ConvertExcelToOnePagePerSheet { get; set; }
        public string ImageBaseUrl { get; set; }
        public double HeaderFooterLogoResizedWidth { get; set; }
        public string PackageOutputPath { get; set; }

        public HeaderFooterConfig HeaderFooterConfig { get; set; }
        public MailSetting PackageCompletedMailSetting { get; set; }

        public MailSetting PackageFailedMailSetting { get; set; }
    }
}