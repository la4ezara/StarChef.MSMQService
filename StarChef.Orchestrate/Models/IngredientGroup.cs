using StarChef.Common;
using System.Data.SqlClient;
using Fourth.Orchestration.Model.Menus;
using System;

namespace StarChef.Orchestrate.Models
{
    public class IngredientGroup
    {
        public string ExternalId { get; set; }
        public string GroupName { get; set; }
    }
}