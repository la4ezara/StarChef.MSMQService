using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Fourth.Orchestration.Model.Menus;
using StarChef.Common;
using StarChef.Orchestrate.Models;

namespace StarChef.Orchestrate
{
    public static class InitDeleteEventExtensions
    {
        public static bool SetBuilderForDelete(this Events.IngredientUpdated.Builder builder, string entityExternalId, int databaseId)
        {
            if (builder == null) return false;

            return true;
        }
        
        public static bool SetBuilderForDelete(this Events.RecipeUpdated.Builder builder, string entityExternalId, int databaseId)
        {
            if (builder == null) return false;

            return true;
        }

        public static bool SetBuilderForDelete(this Events.MenuUpdated.Builder builder, string entityExternalId, int databaseId)
        {
            if (builder == null) return false;

            return true;
        }

        public static bool SetBuilderForDelete(this Events.GroupUpdated.Builder builder, string entityExternalId, int databaseId)
        {
            if (builder == null) return false;

            return true;
        }

        public static bool SetBuilderForDelete(this Events.MealPeriodUpdated.Builder builder, string entityExternalId, int databaseId)
        {
            if (builder == null) return false;

            return true;
        }

        public static bool SetBuilderForDelete(this Events.SupplierUpdated.Builder builder, string entityExternalId, int databaseId)
        {
            if (builder == null) return false;

            return true;
        }

        public static bool SetBuilderForDelete(this Events.UserUpdated.Builder builder, string entityExternalId, int databaseId)
        {
            if (builder == null) return false;

            return true;
        }
    }
}