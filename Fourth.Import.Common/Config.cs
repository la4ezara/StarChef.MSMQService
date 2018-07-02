using System;
using System.Configuration;
using System.IO;
using Fourth.Import.ExcelService;

namespace Fourth.Import.Common
{
    public class Config
    {
        public string TemplateVersion { get; set; }
        public string TemplateHeaderColumn { get; set; }
        public string TemplateHeaderName { get; set; }
        public int ProductDataStartsFrom { get; set; }
        public string ImportType { get; set;}
        public string LoginConnectionString { get; set; }
        public string TargetConnectionString { get; private set; }
        public string ExcelImportConnectionString { get; set; }
        public string FileName { get; private set; }
        public string DisplayFileName { get; private set; }
        public string ExcelImportSheetName { get; set; }
        public int ImportingUserId { get; private set; }
        public int ImportingUgroupId { get; private set; }
        public int IngredientImportId { get; private set; }
        //TODO: Remove FailedRows and ProcessedRows from config if possible. We need this values for each processedFile and it is being used in update status.
        public int FailedRows { get; set; }
        public int ProcessedRows { get; set; }
        public string ExceptionReportFileName { get; set; }

        public Config(string targetConnString, int importId,int userId,int ugroupId,string fileName,string displayFileName)
        {
            LoginConnectionString = ConfigurationManager.ConnectionStrings["StarchefLogin"].ToString();
            TargetConnectionString = targetConnString;
            ExcelImportConnectionString = String.Format(ConfigurationManager.ConnectionStrings["excelImportSheet"].ToString(), fileName, Convert.ToChar(34));
            ExcelImportSheetName = GetImportSheetName(this);
            TemplateVersion = ConfigurationManager.AppSettings["templateVersion"];
            TemplateHeaderColumn = ConfigurationManager.AppSettings["columnHeader"];
            TemplateHeaderName = ConfigurationManager.AppSettings["columnHeaderName"];
            TemplateHeaderName = ConfigurationManager.AppSettings["columnHeaderName"];
            FileName = new FileInfo(fileName).Name;
            ExceptionReportFileName = Guid.NewGuid() + ".xls";
            ProductDataStartsFrom = Convert.ToInt32(ConfigurationManager.AppSettings["dataStartsFromRow"]) - 1; // zero indexed, hence decrease by 1
            ImportType = GetImportType(this);
            ImportingUserId = userId;
            ImportingUgroupId = ugroupId;
            IngredientImportId = importId;
            DisplayFileName = displayFileName;
        }

        private static string GetImportSheetName(Config config)
        {
            return new ImportSheetService(config.ExcelImportConnectionString).ImportSheet();
        }


        private static string GetImportType(Config config)
        {
            return new VersionService(config.ExcelImportConnectionString).TemplateVersion(config.ExcelImportSheetName, config.TemplateVersion);
        }


    }
}