#region usings

using System;

#endregion

namespace Fourth.Starchef.Packages.Model
{
    public class Item
    {
        public String Name { get; set; }
        public int Order { get; set; }
        public AuxDocument AuxiliaryDocument { get; set; }
        public ItemType ItemType { get; set; }
        public ReportFilter ReportFilter { get; set; }
        public int StartPageNumber { get; set; }
        public int EndPageNumber { get; set; }
    }
}