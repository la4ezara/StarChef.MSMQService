using System.Collections.Generic;
using static Fourth.StarChef.Invariables.Constants;

namespace StarChef.Common.Model
{
    /// <summary>
    /// #parts
    /// </summary>
    public class Parts : IEqualityComparer<Parts>
    {
        public Parts() { }
        public Parts(Parts parts)
        {
            EndDishId = parts.EndDishId;
            DishId = parts.DishId;
            IngredientId = parts.IngredientId;
            Quantity = parts.Quantity;
            UnitId = parts.UnitId;
            Level = parts.Level;
            Processed = parts.Processed;
            PartId = parts.PartId;
            Ratio = parts.Ratio;
            RecipeTypeId = parts.RecipeTypeId;
            PortionType = parts.PortionType;
        }

        public int EndDishId { get; set; }
        public int DishId { get; set; }
        public int IngredientId { get; set; }
        public decimal Quantity { get; set; }
        public int UnitId { get; set; }
        public int Level { get; set; }
        public int Processed { get; set; }
        public int PartId { get; set; }
        public decimal Ratio { get; set; }
        //public RecipeType RecipeTypeId { get; set; }
        public int RecipeTypeId { get; set; }
        public bool IsChoise { get; set; }

        public PortionType? PortionType { get; set; }

        public bool Equals(Parts x, Parts y)
        {
            if (x.EndDishId.Equals(y.EndDishId) && x.DishId.Equals(y.DishId)
                && x.IngredientId.Equals(y.IngredientId)
                && x.Quantity.Equals(y.Quantity)
                && x.UnitId.Equals(y.UnitId)
                && x.Level.Equals(y.Level)
                && x.PartId.Equals(y.PartId)
                && x.Ratio.Equals(y.Ratio)
                && x.RecipeTypeId.Equals(y.RecipeTypeId)
                && x.IsChoise.Equals(y.IsChoise)
                && x.PortionType.Equals(y.PortionType)
                )
            {
                return true;
            }

            return false;
        }

        public int GetHashCode(Parts obj)
        {
            return obj.EndDishId.GetHashCode() ^ obj.DishId.GetHashCode()
                ^ obj.IngredientId.GetHashCode() ^ obj.Quantity.GetHashCode()
                ^ obj.UnitId.GetHashCode() ^ obj.Level.GetHashCode()
                ^ obj.PartId.GetHashCode() ^ obj.Ratio.GetHashCode()
                ^ obj.RecipeTypeId.GetHashCode() ^ obj.IsChoise.GetHashCode() ^ obj.PortionType.GetHashCode();
        }
    }
}
