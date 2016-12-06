using System.Collections.Generic;

namespace StarChef.Orchestrate.Models
{
    public class CategoryType
    {
        public int ProductTagId { get; set; }
        public string ExternalId { get; set; }
        public string Name { get; set; }
        public string ParentExternalId { get; set; }
        public int? CategoryExportType { get; set; }
        public bool IsFood { get; set; }
        public List<Category> MainCategories { get; set; }
    }
}