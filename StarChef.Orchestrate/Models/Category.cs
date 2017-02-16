using System.Collections.Generic;
using System.Diagnostics;

namespace StarChef.Orchestrate.Models
{
    [DebuggerDisplay(@"\{{Name}\}")]
    public class Category
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ExternalId { get; set; }
        public string Name { get; set; }
        public string ParentExternalId { get; set; }
        public List<Category> SubCategories { get; set; }
    }

    public class CategoryType : Category
    {
        public int? CategoryExportType { get; set; }
        public bool IsFood { get; set; }
        public List<Category> MainCategories { get; set; }
    }
}