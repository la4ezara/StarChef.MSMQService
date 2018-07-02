#region usings

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using Fourth.Starchef.Packages.Model;
using Fourth.Starchef.Packages.Pdf;

#endregion

namespace Fourth.Starchef.Packages.Manager
{
    public class PackageManager
    {
        public void Process(Config config, Model.Package package, string convertedFolderPath)
        {
            int startPageNumber = 1;
            foreach (Section section in package.Sections.OrderBy(s => s.Order))
            {
                if (package.Pagination == Pagination.BySection)
                    startPageNumber = 1;

                foreach (Item item in section.Items.OrderBy(i => i.Order))
                {
                    if (item.ItemType == ItemType.AuxiliaryDocument)
                    {
                        
                        if (item.AuxiliaryDocument.ConvertedFile != null)
                        {   
                            item.AuxiliaryDocument.PageCount = TotalPages.GetCount(convertedFolderPath + item.AuxiliaryDocument.ConvertedFile);
                        }

                        if (item.AuxiliaryDocument.PageCount <= 0) continue;

                        item.StartPageNumber = startPageNumber;
                        item.EndPageNumber = startPageNumber + item.AuxiliaryDocument.PageCount - 1;
                        startPageNumber = item.EndPageNumber + 1;
                    }
                    else
                    {
                        item.ReportFilter.PageCount =
                            TotalPages.GetCount(config.ReportOutputPath + item.ReportFilter.ReportPath);
                        if (item.ReportFilter.PageCount <= 0)
                        {
                            item.ReportFilter.ReportPageNumberXml = null;
                            continue;
                        }

                        item.StartPageNumber = startPageNumber;
                        item.EndPageNumber = startPageNumber + item.ReportFilter.PageCount - 1;
                        startPageNumber = item.EndPageNumber + 1;
                        item.ReportFilter.ReportPageNumbers =
                            GetReportPageNumbersCollection(item.ReportFilter.ReportPageNumberXml);
                    }
                }
            }
        }

        private static IEnumerable<ReportPageNumber> GetReportPageNumbersCollection(string reportsXml)
        {
            if (string.IsNullOrWhiteSpace(reportsXml))
                return null;

            IEnumerable<ReportPageNumber> reportPageNumbers = null;
            XElement elements = XDocument.Parse(reportsXml).Root;

            if (elements != null)
            {
                reportPageNumbers = from entities in elements.Descendants()
                    let entityName = entities.Element("EntityName")
                    where entityName != null
                    select new ReportPageNumber
                    {
                        EntityName = entityName.Value,
                        PageNumber = (int) entities.Element("PageNumber")
                    };
            }
            return reportPageNumbers;
        }
    }
}