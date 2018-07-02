#region usings

using System;
using System.Collections.Generic;

#endregion

namespace Fourth.Starchef.Packages.Model
{
    public class ReportFilter
    {
        public ReportFilter()
        {
            ReportQueueGuid = Guid.NewGuid();
        }

        public Guid ReportQueueGuid { get; private set; }
        public int ReportId { get; set; }
        public string Name { get; set; }
        public int GroupFilterId { get; set; }
        public int ScopeFilterId { get; set; }
        public int GroupFilterType { get; set; }
        public int FilterType { get; set; }
        public string Filters { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string ReportPath { get; set; }
        public string ReportPageNumberXml { get; set; }
        public IEnumerable<ReportPageNumber> ReportPageNumbers { get; set; }
        public ReportingEngine ReportingEngine { get; set; }
        public int PageCount { get; set; }
        public bool NoData { get; set; }
    }
}