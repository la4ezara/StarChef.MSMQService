using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarChef.Orchestrate.Models
{
    public class RecipeIngredient
    {
        public int RecipeId { get; set; }
        public string IngredientExternalId { get; set; }
        public string IngredientName { get; set; }
        public double ProductMeasure { get; set; }
        public string ProductUom { get; set; }
        public int ProductPartId { get; set; }
    }
}
