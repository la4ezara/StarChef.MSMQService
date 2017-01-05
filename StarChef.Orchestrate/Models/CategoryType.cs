using System.Collections.Generic;

namespace StarChef.Orchestrate.Models
{
    public class CategoryType: Category
    {
        public int? CategoryExportType { get; set; }
        public bool IsFood { get; set; }
        public List<Category> MainCategories { get; set; }
    }
}