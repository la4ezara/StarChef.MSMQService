using System.Collections.Generic;

namespace StarChef.Orchestrate.Models
{
    public class Category
    {
        public string ExternalId { get; set; }
        public string Name { get; set; }
        public string ParentExternalId { get; set; }
        public List<Category> SubCategories { get; set; }
    }
}