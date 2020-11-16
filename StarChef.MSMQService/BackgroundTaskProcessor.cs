using Fourth.StarChef.Invariables;
using Fourth.StarChef.Invariables.Interfaces;
using log4net;
using Newtonsoft.Json;
using StarChef.Common;
using StarChef.Common.Engine;
using StarChef.Common.Extensions;
using StarChef.Common.Repository;
using StarChef.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace StarChef.MSMQService
{
    public class BackgroundTaskProcessor
    {
        private readonly string _connectionString;
        private readonly int _organizationId;
        private readonly IDatabaseManager _databaseManager;
        private readonly ILog _logger;
        private readonly IPriceEngine _priceEngine;

        public BackgroundTaskProcessor(int organizationId, string connectionStr, IDatabaseManager databaseManager, IPriceEngine priceEngine, ILog logger)
        {
            _connectionString = connectionStr;
            _organizationId = organizationId;
            _databaseManager = databaseManager;
            _priceEngine = priceEngine;
            _logger = logger;
        }

        public void ProcessMessage(IBackgroundTask task)
        {
            if (task != null)
            {
                _logger.InfoFormat("Received MSMQ message: {0}", task.ToJson());

                switch (task.TaskType)
                {
                    case Constants.MessageActionType.UpdatedUserDefinedUnit:
                        ProcessUduUpdate(task);
                        break;
                    case Constants.MessageActionType.UpdatedProductSet:
                        ProcessProductSetUpdate(task);
                        break;
                    case Constants.MessageActionType.UpdatedPriceBand:
                        ProcessPriceBandUpdate(task);
                        break;
                    case Constants.MessageActionType.UpdatedGroup:
                        ProcessGroupUpdate(task);
                        //extend processing to orchestration
                        task.TaskType = Constants.MessageActionType.StarChefEventsUpdated;
                        task.EntityType = Constants.EntityType.Group;
                        task.ProductId = task.GroupId;
                        ProcessStarChefEventsUpdated(task);
                        break;
                    case Constants.MessageActionType.UpdatedProductCost:
                        ProcessProductCostUpdate(task);
                        break;
                    case Constants.MessageActionType.GlobalUpdate:
                        ProcessGlobalUpdate(task);
                        break;
                    case Constants.MessageActionType.UpdatedProductNutrient:
                        ProcessProductNutrientUpdate(task);

                        //extend processing to orchestration
                        task.TaskType = Constants.MessageActionType.UpdatedProductNutrient;
                        if (task.EntityType == Constants.EntityType.Dish || task.EntityType == Constants.EntityType.Ingredient)
                        {
                            if (task.EntityType == Constants.EntityType.Dish)
                            {
                                ProcessStarChefEventsUpdated(task);
                            }

                            this.AddOrchestrationMessageForAffectedRecipes(task.ProductId, _connectionString);
                        }
                        else if ((int)task.EntityType == 0)
                        {
                            task.EntityType = Constants.EntityType.Dish;
                            ProcessStarChefEventsUpdated(task);
                        }
                        break;
                    case Constants.MessageActionType.UpdatedProductIntolerance:
                        ProcessProductIntoleranceUpdate(task);
                        break;
                    case Constants.MessageActionType.UpdatedProductNutrientInclusive:
                        ProcessProductNutrientInclusiveUpdate(task);
                        break;
                    case Constants.MessageActionType.GlobalUpdateBudgeted:
                        ProcessGlobalUpdateBudgeted(task);
                        break;
                    case Constants.MessageActionType.UpdateAlternateIngredients:
                        ProcessAlternateIngredientUpdate(task);
                        break;
                    // All Events are populating under StarChefEventsUpdated Action - Additional action added for 
                    // User because of multiple different actions
                    case Constants.MessageActionType.StarChefEventsUpdated:
                    // Starchef to Salesforce - later Salesforce notify to Starchef the user created notification
                    case Constants.MessageActionType.UserCreated:
                    case Constants.MessageActionType.UserUpdated:
                    case Constants.MessageActionType.UserActivated:
                    // Once user created in Salesforce, SF will notified and to SC and SC store the external id on DB
                    case Constants.MessageActionType.SalesForceUserCreated:
                        _logger.Debug("enter StarChefEventsUpdated");
                        ProcessStarChefEventsUpdated(task);
                        _logger.Debug("exit StarChefEventsUpdated");
                        break;
                    case Constants.MessageActionType.EntityImported:
                        PostProcessingPerSubAction(task);
                        break;
                    case Constants.MessageActionType.UpdatedInventoryValidation:
                        ProcessUpdatedInventoryValidation(_connectionString);
                        break;
                    case Constants.MessageActionType.UpdatedProductABV:
                        ProcessProductAbvUpdate(_connectionString, task.ProductId, task.TrackId, task.UserId);
                        break;
                    case Constants.MessageActionType.EnabledAbv:
                        ProcessFullAbvIngredientRecalculation(_connectionString, task.UserId);
                        break;
                    case Constants.MessageActionType.UpdateAlternatesRank:
                        ProcessRankReorder(_connectionString, task.ExtendedProperties);
                        break;
					case Constants.MessageActionType.UpdatedProductFIR:
						ProcessProductFIRUpdate(_connectionString, task.ProductId, task.TrackId, task.UserId);
						break;
					case Constants.MessageActionType.EnabledFIR:
						ProcessFullFIRIngredientRecalculation(_connectionString, task.UserId);
						break;

				}
            }
        }

        private void PostProcessingPerSubAction(IBackgroundTask task)
        {
            var importSettings = _databaseManager.GetImportSettings(_connectionString, _organizationId);
            switch (task.SubTaskType)
            {
                case Constants.MessageSubActionType.ImportedIngredient:

                    #region Ingredient
                    {
                        var importTypeSettings = importSettings.Ingredient();
                        ExecuteStoredProc(_connectionString, "sc_product_run_rankreorder", new SqlParameter("@product_id", task.ProductId));
                        //check when to trigger price recalculation for each affected item
                        if (importTypeSettings.AutoCalculateCost)
                        {
                            ProcessPriceRecalculation(_connectionString, 0, task.ProductId, 0, 0, 0, task.CreateDate);
                        }

                        //when import new alternate/ingredient we should copy ingredient values to alternates in terms of nutrition and intolerances.
                        ProcessImportAlternateIngredientUpdate(task);
                    }
                    #endregion

                    break;
                case Constants.MessageSubActionType.ImportedIngredientIntolerance:

                    #region IngredientIntolerance

                    {
                        var importTypeSettings = importSettings.IngredientIntolerance();

                        if (importTypeSettings.AutoCalculateIntolerance)
                        {
                            int userId = 1;
                            if (task.UserId != 0)
                            {
                                userId = task.UserId;
                            }

                            ExecuteStoredProc(_connectionString, "sc_audit_log_nutrition_intolerance",
                                new SqlParameter("@product_id", task.ProductId),
                                new SqlParameter("@db_entity_id", 20), // hardcoded
                                new SqlParameter("@user_id", userId), // hardcoded
                                new SqlParameter("@audit_type_id", 6) // hardcoded
                                );

                            ExecuteStoredProc(_connectionString, "utils_recalc_parent_intol_by_product",
                                new SqlParameter("@product_id", task.ProductId),
                                new SqlParameter("@user_id", userId));

                            ExecuteStoredProc(_connectionString, "sc_batch_product_labelling_update",
                                new SqlParameter("@product_id", task.ProductId),
                                new SqlParameter("@msmq_log_id", task.TrackId),
                                new SqlParameter("@user_id", userId));
                        }

                        //when import new alternate/ingredient we should copy ingredient values to alternates in terms of nutrition and intolerances.
                        ProcessImportAlternateIngredientUpdate(task);
                    }

                    #endregion

                    break;
                case Constants.MessageSubActionType.ImportedIngredientNutrient:

                    #region IngredientNutrient

                    {
                        var importTypeSettings = importSettings.IngredientNutrient();
                        if (importTypeSettings.AutoCalculateIntolerance)
                        {
                            ExecuteStoredProc(_connectionString, "sc_audit_log_nutrition_intolerance",
                                new SqlParameter("@product_id", task.ProductId),
                                new SqlParameter("@db_entity_id", 20), // hardcoded
                                new SqlParameter("@user_id", 1), // hardcoded
                                new SqlParameter("@audit_type_id", 6) // hardcoded
                                );

                            // first try to recalculate fibre if need
                            string fiberFlag;
                            var properties = task.ExtendedProperties.Pairs();
                            if (properties.TryGetValue("FIBER_RECALC_REQUIRED", out fiberFlag) && Convert.ToBoolean(fiberFlag))
                            {
                                ExecuteStoredProc(_connectionString, "_sc_recalculate_nutrient_fibre", new SqlParameter("@product_id", task.ProductId));
                            }

                            //calculate summary for ingredient
                            ExecuteStoredProc(_connectionString, "_sc_update_calorie_details", new SqlParameter("@product_id", task.ProductId));

                            //calculate nutrition data for related recipes
                            ExecuteStoredProc(_connectionString, "_sc_update_dish_yield",
                                new SqlParameter("@product_id", task.ProductId),
                                new SqlParameter("@include_self", false) // hardcoded
                                );
                        }

                        //when import new alternate/ingredient we should copy ingredient values to alternates in terms of nutrition and intolerances.
                        ProcessImportAlternateIngredientUpdate(task);
                    }

                    #endregion

                    break;
                case Constants.MessageSubActionType.ImportedIngredientPriceBand:

                    #region IngredientPriceBand
                    
                    var importPriceBandSettings = importSettings.IngredientPriceBand();
                    if (importPriceBandSettings.AutoCalculateCost)
                    {
                        if (task.ProductId > 0)
                        {
                            //if empty trigger global price recalc
                            ProcessPriceRecalculation(_connectionString, 0, task.ProductId, 0, 0, 0, task.CreateDate);
                        }
                        else
                        {
                            ProcessPriceRecalculation(_connectionString, 0, 0, 0, 0, 0, task.CreateDate);
                        }
                    }
                    
                    #endregion

                    break;
                case Constants.MessageSubActionType.ImportedIngredientConversion:
                case Constants.MessageSubActionType.ImportedIngredientAlternateSwitch:
                    #region IngredientConversion

                    {
                        ExecuteStoredProc(_connectionString, "sc_audit_history_single_log",
                            new SqlParameter("@entity_id", task.ProductId),
                            new SqlParameter("@modified_columns", "Pack Size"),
                            new SqlParameter("@db_entity_id", 20)); // hardcoded

                        //when import new alternate/ingredient we should copy ingredient values to alternates in terms of nutrition and intolerances.
                        ProcessImportAlternateIngredientUpdate(task, true);
                    }

                    #endregion

                    break;
                case Constants.MessageSubActionType.ImportedIngredientAlternatesReorder:
                    #region IngredientAlternatesReorder
                    {
                        ProcessPriceRecalculation(_connectionString, 0, task.ProductId, 0, 0, 0, task.CreateDate);
                        ProcessProductNutrientUpdate(task);
                        ProcessProductIntoleranceUpdate(task);
                        ProcessProductAbvUpdate(_connectionString, task.ProductId, task.TrackId, task.UserId);
                    }

                    #endregion
                    break;
                case Constants.MessageSubActionType.ImportedIngredientCost:
                    #region IngredientCost
                    {
                        ProcessPriceRecalculation(_connectionString, 0, task.ProductId, 0, 0, 0, task.CreateDate);
                    }

                    #endregion
                    break;
                case Constants.MessageSubActionType.ImportedRecipe:
                    break;
                case Constants.MessageSubActionType.ImportedRecipeIngredients:
                    ProcessImportRecipeIngredients(task);
                    break;
                case Constants.MessageSubActionType.ImportedIngredientFIR:
                    ProcessImportRecipeIngredientsFIR(task);
                    break;
                case Constants.MessageSubActionType.ImportedUsers:
                case Constants.MessageSubActionType.ImportedIngredientCategory:
                default:
                    // do nothing
                    break;
            }
        }

        private void ProcessUduUpdate(IBackgroundTask task)
        {
            ProcessPriceRecalculation(_connectionString, 0, 0, 0, 0, task.ProductId, task.CreateDate);
        }

        private void ProcessProductSetUpdate(IBackgroundTask task)
        {
            ProcessPriceRecalculation(_connectionString, 0, 0, task.ProductId, 0, 0, task.CreateDate);
        }

        private void ProcessPriceBandUpdate(IBackgroundTask task)
        {
            ProcessPriceRecalculation(_connectionString, 0, 0, 0, task.ProductId, 0, task.CreateDate);
        }

        private void ProcessGroupUpdate(IBackgroundTask task)
        {
            ProcessPriceRecalculation(_connectionString, task.GroupId, 0, 0, 0, 0, task.CreateDate);
        }

        private void ProcessProductCostUpdate(IBackgroundTask task)
        {
            ProcessPriceRecalculation(_connectionString, 0, task.ProductId, 0, 0, 0, task.CreateDate);
        }

        private void ProcessProductNutrientUpdate(IBackgroundTask task)
        {
            ExecuteStoredProc(_connectionString,
                "sc_batch_product_nutrition_update",
                new SqlParameter("@product_id", task.ProductId));
        }

        private void ProcessProductNutrientInclusiveUpdate(IBackgroundTask task)
        {
            ExecuteStoredProc(_connectionString,
                "sc_batch_product_nutrition_update_inc",
                new SqlParameter("@product_id", task.ProductId));
        }

        private void ProcessProductIntoleranceUpdate(IBackgroundTask task)
        {
            ExecuteStoredProc(_connectionString,
                "sc_batch_product_labelling_update",
                new SqlParameter[] {
                    new SqlParameter("@product_id", task.ProductId),
                    new SqlParameter("@msmq_log_id", task.TrackId),
                    new SqlParameter("@user_id", task.UserId)
                });
        }

        private void ProcessGlobalUpdate(IBackgroundTask task)
        {
            ProcessPriceRecalculation(_connectionString, task.GroupId, 0, 0, 0, 0, task.CreateDate);
        }

        private void ProcessGlobalUpdateBudgeted(IBackgroundTask task)
        {
            ExecuteStoredProc(_connectionString, "sc_calculate_dish_pricing_budgeted");
        }

        private void ProcessAlternateIngredientUpdate(IBackgroundTask task)
        {
            ExecuteStoredProc(_connectionString, "sc_alternate_ingredient_update", new SqlParameter[] {
                new SqlParameter("@product_id", task.ProductId),
                new SqlParameter("@user_id", task.UserId)});
        }

        private void ProcessImportAlternateIngredientUpdate(IBackgroundTask task)
        {
            ProcessImportAlternateIngredientUpdate(task, false);
        }

        private void ProcessImportAlternateIngredientUpdate(IBackgroundTask task, bool forceRecalculation)
        {
            ExecuteStoredProc(_connectionString, "import_api_alternate_ingredient_update",
                new SqlParameter("@product_id", task.ProductId),
                new SqlParameter("@user_id", task.UserId),
                new SqlParameter("@force_recalculation", Convert.ToInt16(forceRecalculation)));
        }

        private void ProcessImportRecipeIngredients(IBackgroundTask task)
        {
            ProcessProductIntoleranceUpdate(task);
            ProcessProductNutrientUpdate(task);
            ProcessProductAbvUpdate(_connectionString, task.ProductId, task.TrackId, task.UserId);
            ProcessPriceRecalculation(_connectionString, 0, task.ProductId, 0, 0, 0, task.CreateDate);
			ProcessProductFIRUpdate(_connectionString, task.ProductId, 0, task.UserId);

            var isOrchestrationEnabled = _databaseManager.IsPublishEnabled(_connectionString, (int)task.EntityType);
            if (isOrchestrationEnabled)
            {
                AddOrchestrationMessageToQueue(_connectionString, task.ProductId, (int)task.EntityType, task.ExternalId, Constants.MessageActionType.StarChefEventsUpdated);
            }
        }

        private void ProcessImportRecipeIngredientsFIR(IBackgroundTask task)
        {
            ProcessProductFIRUpdate(_connectionString, task.ProductId, 0, task.UserId);

            var isOrchestrationEnabled = _databaseManager.IsPublishEnabled(_connectionString, (int)task.EntityType);
            if (isOrchestrationEnabled)
            {
                AddOrchestrationMessageToQueue(_connectionString, task.ProductId, (int)task.EntityType, task.ExternalId, Constants.MessageActionType.StarChefEventsUpdated);
            }
        }

        private void ProcessStarChefEventsUpdated(IBackgroundTask task)
        {
            var entityTypeId = 0;
            var entityId = 0;

            var productIds = new List<int>();
            switch (task.EntityType)
            {
                case Constants.EntityType.User:
                    entityTypeId = (int)Constants.EntityType.User;
                    entityId = task.ProductId;
                    break;
                case Constants.EntityType.UserGroup:
                    entityTypeId = (int)Constants.EntityType.UserGroup;
                    entityId = task.ProductId;
                    break;
                case Constants.EntityType.Ingredient:
                    entityTypeId = (int)Constants.EntityType.Ingredient;
                    entityId = task.ProductId;
                    if (!string.IsNullOrEmpty(task.ExtendedProperties))
                    {
                        productIds = JsonConvert.DeserializeObject<List<int>>(task.ExtendedProperties);
                    }
                    break;
                case Constants.EntityType.Dish:
                    entityTypeId = (int)Constants.EntityType.Dish;
                    entityId = task.ProductId;
                    if (!string.IsNullOrEmpty(task.ExtendedProperties))
                    {
                        productIds = JsonConvert.DeserializeObject<List<int>>(task.ExtendedProperties);
                    }
                    break;
                case Constants.EntityType.Menu:
                    entityTypeId = (int)Constants.EntityType.Menu;
                    entityId = task.ProductId;
                    break;
                case Constants.EntityType.MealPeriodManagement:
                    entityTypeId = (int)Constants.EntityType.MealPeriodManagement;
                    entityId = task.ProductId;
                    break;
                case Constants.EntityType.Group:
                    entityTypeId = (int)Constants.EntityType.Group;
                    entityId = task.ProductId;
                    break;
                case Constants.EntityType.Supplier:
                    entityTypeId = (int)Constants.EntityType.Supplier;
                    entityId = task.ProductId;
                    break;
                case Constants.EntityType.ProductSet:
                    entityTypeId = (int)Constants.EntityType.ProductSet;
                    entityId = task.ProductId;
                    break;
            }

            var isOrchestrationEnabled = _databaseManager.IsPublishEnabled(_connectionString, entityTypeId);
            if (isOrchestrationEnabled)
            {
                if (!productIds.Any())
                {
                    _logger.Debug("enter send");
                    AddOrchestrationMessageToQueue(_connectionString, entityId, entityTypeId, task.ExternalId, task.TaskType);
                    _logger.Debug("exit send");
                }
                else
                {
                    foreach (int id in productIds)
                    {
                        AddOrchestrationMessageToQueue(_connectionString, id, entityTypeId, string.Empty,task.TaskType);
                    }
                }
            }
        }
        private int ExecuteStoredProc(string connectionString, string spName, params SqlParameter[] parameterValues)
        {
            var result = _databaseManager.Execute(connectionString, spName, Constants.TIMEOUT_MSMQ_EXEC_STOREDPROC, true, parameterValues);
            return result;
        }

        private void ProcessUpdatedInventoryValidation(string dsn)
        {
            ExecuteStoredProc(dsn, "sc_switch_invisible_validation");
        }

        private void ProcessProductAbvUpdate(string dsn, int productId, int trackId, int userId)
        {
            ExecuteStoredProc(dsn,
                "sc_batch_product_abv_update",
                new SqlParameter[] {
                    new SqlParameter("@product_id", productId),
                    new SqlParameter("@msmq_log_id", trackId),
                    new SqlParameter("@user_id", userId)
                });
        }

        private void AddOrchestrationMessageToQueue(string dsn, int entityId, int entityTypeId, string externalId, Constants.MessageActionType messageActionTypeId)
        {

            ExecuteStoredProc(dsn,
                "sc_calculation_enqueue",
                new SqlParameter("@EntityId", entityId),
                new SqlParameter("@EntityTypeId", entityTypeId),
                new SqlParameter("@RetryCount", 0) { Value = 0, DbType = System.Data.DbType.Int32 },
                new SqlParameter("@StatusId", 1),
                new SqlParameter("@DateCreated", DateTime.UtcNow),
                new SqlParameter("@ExternalId", externalId),
                new SqlParameter("@MessageActionTypeId", messageActionTypeId));
        }

        public string GetCustomerFromDsn(string dsn)
        {
            string result = string.Empty;
            var dsnParts = dsn.ToLower().Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            var customer = dsnParts.FirstOrDefault(x => x.Contains("initial catalog"));
            if (!string.IsNullOrEmpty(customer) && customer.Contains("="))
            {
                var segments = customer.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (segments.Length > 1)
                {
                    result = customer.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries).Last();
                }
            }
            return result;
        }

        public void ProcessFullAbvIngredientRecalculation(string dsn, int userId)
        {
            ExecuteStoredProc(dsn,
                 "sc_full_recipe_abv_recalculation",
                 new SqlParameter[] {
                    new SqlParameter("@user_id", userId)
                 });

        }

        public void ProcessRankReorder(string dsn, string extendedProperties)
        {
            var repo = new IngredientRepository(_databaseManager);
            var productIds = JsonConvert.DeserializeObject<List<int>>(extendedProperties);

            foreach (var productId in productIds)
            {
                repo.RunRankReorder(productId, dsn);
            }
        }


        private void ProcessPriceRecalculation(string dsn, int groupId, int productId, int psetId, int pbandId, int unitId, DateTime arrivedTime)
        {
            var customer = GetCustomerFromDsn(dsn);

            _logger.Info($"{customer} sc_calculate_dish_pricing @group_id = {groupId}, @product_id = {productId}, @pset_id = {psetId}, @pband_id = {pbandId}, @unit_id = {unitId}, @message_arrived_time = {arrivedTime.ToString()}");
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            //var engine = GetPriceEngine(dsn);
            bool newPriceEngineOn = _priceEngine.IsEngineEnabled().Result;
            bool runGlobalRecalculation = true;

            if (productId > 0)
            {
                runGlobalRecalculation = false;
            }

            //always run old algo for product price recalculation
            if (newPriceEngineOn)
            {

                _logger.Info($"{customer} New engine is used");
                _logger.Info($"Recalculate Product {productId}");
                sw.Start();
                IEnumerable<Common.Model.DbPrice> result;
                if (runGlobalRecalculation)
                {
                    result = _priceEngine.GlobalRecalculation(true, groupId, pbandId, psetId, unitId, arrivedTime).Result;
                }
                else
                {
                    result = _priceEngine.Recalculation(productId, groupId, pbandId, psetId, unitId, true, arrivedTime).Result;
                }

                sw.Stop();
                _logger.Info($"{customer} New engine generate {result.Count()} prices for {sw.Elapsed.TotalSeconds}");
            }
            else
            {
                _logger.Info($"{customer} Old engine is used");
                sw.Start();
                ExecuteStoredProc(dsn,
                    "sc_calculate_dish_pricing",
                    new SqlParameter("@group_id", groupId),
                    new SqlParameter("@product_id", productId),
                    new SqlParameter("@pset_id", psetId),
                    new SqlParameter("@pband_id", pbandId),
                    new SqlParameter("@unit_id", unitId),
                    new SqlParameter("@message_arrived_time", arrivedTime));
                sw.Stop();
                _logger.Info($"{customer} Old engine finish in {sw.Elapsed.TotalSeconds}");
            }
        }

        private void AddOrchestrationMessageForAffectedRecipes(int productId, string connectionString)
        {

            var isRecipeOrchestrationEnabled = _databaseManager.IsPublishEnabled(connectionString, (int)Constants.EntityType.Dish);
            if (isRecipeOrchestrationEnabled)
            {
                var affectedRecipies = _databaseManager.Query<int>(connectionString, "sc_list_usage_affectedProducts", new { product_id = productId }, CommandType.StoredProcedure);

                foreach (var recipeId in affectedRecipies)
                {
                    var parms1 = new SqlParameter[2];
                    parms1[0] = new SqlParameter("@entity_id", recipeId);
                    parms1[1] = new SqlParameter("@message_type", (int)Constants.MessageActionType.UpdatedProductNutrient);
                    ExecuteStoredProc(connectionString, "add_affected_recipe_entity_to_orchestration_queue", parms1);
                }
            }
        }

		private void ProcessProductFIRUpdate(string dsn, int productId, int trackId, int userId)
		{
			ExecuteStoredProc(dsn,
				"sc_batch_product_fir_update",
				new SqlParameter[] {
					new SqlParameter("@product_id", productId),
					new SqlParameter("@msmq_log_id", trackId),
					new SqlParameter("@user_id", userId)
				});
		}


		public void ProcessFullFIRIngredientRecalculation(string dsn, int userId)
		{
			ExecuteStoredProc(dsn,
				 "sc_full_recipe_fir_recalculation",
				 new SqlParameter[] {
					new SqlParameter("@user_id", userId)
				 });

		}
	}
}
