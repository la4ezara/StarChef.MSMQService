#region usings

using System;
using System.IO;
using System.Net;
using Fourth.Starchef.Packages.DataService;
using Fourth.Starchef.Packages.Model;

#endregion

namespace Fourth.Starchef.Packages.Report
{
    public class StarchefReportingEngine
    {
        public bool Run(Config config, ReportFilter reportFilter, Paper paperSize)
        {
            ReportService reportService = new ReportService();
            bool addReportToQueue = reportService.AddReportToQueue(config, reportFilter, paperSize);
            if (!addReportToQueue) return false;

            string reportResponse;
            if (LoadReport(config, reportFilter, paperSize, out reportResponse))
            {
                string[] reportResultValues = reportResponse.Split(new[] {"-|-"}, StringSplitOptions.None);
                if (reportResultValues.Length == 2)
                {
                    reportFilter.ReportPath = reportResultValues[0];
                    reportFilter.ReportPageNumberXml = reportResultValues[1];
                    return true;
                }
            }
            return false;
        }

        private bool LoadReport(Config config, ReportFilter reportFilter, Paper paperSize, out string reportResponse)
        {
            string reportingEngineUrl;
            switch (reportFilter.ReportingEngine)
            {
                case ReportingEngine.Engine1:
                    reportingEngineUrl = string.Format("{0}{1}&paperSize={2}", config.ReportingEngine1Url, reportFilter.ReportQueueGuid, (int) paperSize);
                    break;
                case ReportingEngine.Engine2:
                    reportingEngineUrl = string.Format("{0}{1}/{2}", config.ReportingEngine2Url, reportFilter.ReportQueueGuid, (int) paperSize);
                    break;
                default:
                    throw new Exception("unknow reporting engine");
            }
            /*
            System.Diagnostics.EventLog eventLog = new System.Diagnostics.EventLog
            {
                Source = "Fourth.Starchef.Packages"
            };
            eventLog.WriteEntry(reportingEngineUrl);
            */
            reportResponse = string.Empty;
            HttpWebRequest http = (HttpWebRequest) WebRequest.Create(reportingEngineUrl);
            http.Timeout = Convert.ToInt32(new TimeSpan(0, 30, 0).TotalMilliseconds);

            HttpWebResponse response = (HttpWebResponse) http.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                using (StreamReader sr = new StreamReader(stream: response.GetResponseStream()))
                {
                    reportResponse = sr.ReadToEnd();
                }
                return true;
            }
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                reportFilter.NoData = true;
            }
            return false;
        }
    }
}