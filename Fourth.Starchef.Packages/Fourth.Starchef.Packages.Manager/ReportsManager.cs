#region usings

using System.Linq;
using Fourth.Starchef.Packages.Model;
using Fourth.Starchef.Packages.Report;

#endregion

namespace Fourth.Starchef.Packages.Manager
{
    public class ReportsManager
    {
        public void Process(Config config, Model.Package package)
        {
            package.LogProgress(package.LogId, "Processing reports");
            foreach (Section section in package.Sections)
            {
                package.LogProgress(package.LogId, "Processing reports in section \"" + section.Name + "\"");
                foreach (Item item in section.Items.Where(i => i.ItemType == ItemType.Report))
                {
                    package.LogProgress(package.LogId, "Generating report \"" + item.Name + "\"");
                    StarchefReportingEngine engine = new StarchefReportingEngine();
                    engine.Run(config, item.ReportFilter, package.PageSetting.Paper);
                }
                package.LogProgress(package.LogId, "Completed processing reports in section \"" + section.Name + "\"");
            }
            package.LogProgress(package.LogId, "Completed generating reports");
        }
    }
}