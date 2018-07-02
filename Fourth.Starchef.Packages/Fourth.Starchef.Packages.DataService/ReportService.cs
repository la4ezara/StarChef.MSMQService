#region usings

using System.Data;
using Fourth.Starchef.Packages.Data;
using Fourth.Starchef.Packages.Model;

#endregion

namespace Fourth.Starchef.Packages.DataService
{
    public class ReportService
    {
        public bool AddReportToQueue(Config config, ReportFilter reportFilter, Paper paperSize)
        {
            using (Dal dal = new Dal(config.LoginConnString))
            {
                IDataParameter[] param =
                {
                    dal.GetParameter("@report_queue_id", reportFilter.ReportQueueGuid),
                    dal.GetParameter("@user_dsn", config.ConnString),
                    dal.GetParameter("@report_id", reportFilter.ReportId),
                    dal.GetParameter("@user_id", config.UserId),
                    dal.GetParameter("@is_complete", 0),
                    dal.GetParameter("@group_filter_id", reportFilter.GroupFilterId),
                    dal.GetParameter("@group_filter_type", reportFilter.GroupFilterType),
                    dal.GetParameter("@scope_filter_id", reportFilter.ScopeFilterId),
                    dal.GetParameter("@filter_type", reportFilter.FilterType),
                    dal.GetParameter("@filter_value", reportFilter.Filters),
                    dal.GetParameter("@start_date", reportFilter.StartDate),
                    dal.GetParameter("@end_date", reportFilter.EndDate),
                    dal.GetParameter("@download_type", "ForPackage"),
                    dal.GetParameter("@is_preview", 0),
                };
                return dal.ExecuteSql("rp_insert_report_queue", param, CommandType.StoredProcedure) > 0;
            }
        }
    }
}