#region usings

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#endregion

namespace Fourth.Starchef.Packages.Model
{
    public class Package
    {
        public Package()
        {
            PdfPackagePageNumbers = new Collection<PdfPackagePageNumber>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public GroupLogo GroupLogo { get; set; }
        public GroupLogo LineSeparator { get; set; }
        public Permission Permission { get; set; }
        public Pagination Pagination { get; set; }
        public bool IncludeToc { get; set; }
        public bool SaveAsDocument { get; set; }
        public ICollection<Section> Sections { get; set; }
        public ICollection<PdfPackagePageNumber> PdfPackagePageNumbers { get; private set; }
        public PageSetting PageSetting { get; set; }
        public HeaderFooter Header { get; set; }
        public HeaderFooter Footer { get; set; }
        public string FontName { get; set; }
        public string CopyrightText { get; set; }
        public string CompanyName { get; set; }
        public int LogId { get; set; }
        public Action<int,string> LogProgress { get; set; }

        
    }
}