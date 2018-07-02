using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Reflection;
using Fourth.Import.Common;
using Fourth.Import.Common.Messaging;
using Fourth.Import.ExcelService;
using Fourth.Import.Model;
using log4net;
using Fourth.StarChef.Invariables;
using Newtonsoft.Json;

namespace Fourth.Import.Process
{
    public class IngredientImport
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Process(FileInfo fileInfo, string sourceLogin)
        {
            _logger.Debug($"Processing file '{fileInfo.Name}'");
            string processingFilePath = ConfigurationManager.AppSettings["ProcessingFilePath"];
            string smallFileName = fileInfo.Name;
            string processingFileName = processingFilePath + smallFileName;
            string processedFileName = ConfigurationManager.AppSettings["ProcessedFilePath"] + smallFileName;

            var loginDb = sourceLogin == "StarChefLogin" ? "StarChefLogin" : "StarChefLoginTrg";
            using (var iiService = new DataService.IngredientImportService(ConfigurationManager.ConnectionStrings[loginDb].ToString()))
            {
                var targetConnectionString = ConfigurationManager.ConnectionStrings["Target_DB"].ToString();
                var databaseInfo = iiService.GetConnectionString(smallFileName, targetConnectionString);
                var databaseId = databaseInfo.Item1;
                var customerDbConnectionString = databaseInfo.Item2;
                _logger.Debug($"Processing file '{fileInfo.Name}' databaseId {databaseId} connectionString'{customerDbConnectionString}'");

                //Changing connection string to user database connection.
                using (var service = new DataService.IngredientImportService(customerDbConnectionString))
                {
                    FileService fileService = new FileService();
                    fileService.MoveFile(fileInfo.FullName, processingFileName);
                    _logger.Debug($"Moved file to processing '{fileInfo.Name}' databaseId {databaseId}");
                    Config config = null;
                    try
                    {
                        config = service.GetImportInformation(customerDbConnectionString, smallFileName, processingFileName);
                        _logger.Debug($"Update status to processing '{fileInfo.Name}' databaseId {databaseId}");
                        service.UpdateStatus(config, smallFileName, ImportStatus.Processing);
                        _logger.Debug($"Processing '{fileInfo.Name}' databaseId {databaseId}");
                        ImportOperation currentOperation = new Start().Import(config);
                        _logger.Debug($"Moved file to processed '{fileInfo.Name}' databaseId {databaseId}");
                        fileService.MoveFile(processingFileName, processedFileName);
                        if (currentOperation == ImportOperation.Insert)
                            service.UpdateStatus(config, smallFileName, ImportStatus.ProcessedNewIngredient);
                        else if (currentOperation == ImportOperation.FuturePrice)
                            service.UpdateStatus(config, smallFileName, ImportStatus.ProcessedPriceUpdate);
                        else if (currentOperation == ImportOperation.InternalFuturePrice)
                            service.UpdateStatus(config, smallFileName, ImportStatus.ProcessedPriceUpdate);
                        else if (currentOperation == ImportOperation.FuturePriceWithInvoiceCostPrice)
                            service.UpdateStatus(config, smallFileName, ImportStatus.ProcessedPriceUpdate);
                        else if (currentOperation == ImportOperation.IntolUpdate)
                        {
                            _logger.Debug($"Updating intolerances '{fileInfo.Name}' databaseId {databaseId}");
                            service.UpdateIntolerances(config);
                            _logger.Debug($"Updated intolerances '{fileInfo.Name}' databaseId {databaseId}");
                            _logger.Debug($"Update status to processed '{fileInfo.Name}' databaseId {databaseId}");
                            service.UpdateStatus(config, smallFileName, ImportStatus.ProcessedIntoleranceUpdate);
                        }
                        else if (currentOperation == ImportOperation.NutritionUpdate)
                        {
                            _logger.Debug($"Updating nutrients '{fileInfo.Name}' databaseId {databaseId}");
                            service.UpdateNutrients(config);
                            _logger.Debug($"Updated nutrients '{fileInfo.Name}' databaseId {databaseId}");
                            _logger.Debug($"Update status to processed '{fileInfo.Name}' databaseId {databaseId}");
                            service.UpdateStatus(config, smallFileName, ImportStatus.ProcessedNutritionUpdate);
                        }
                        else if (currentOperation == ImportOperation.PriceOverride)
                            service.UpdateStatus(config, smallFileName, ImportStatus.ProcessedPriceUpdate);
                        else if (currentOperation == ImportOperation.SupIntolUpdate)
                        {
                            _logger.Debug($"Updating supplier intolerances '{fileInfo.Name}' databaseId {databaseId}");
                            service.UpdateSupplierIntolerances(config);
                            _logger.Debug($"Updated supplier intolerances '{fileInfo.Name}' databaseId {databaseId}");

                            if (config.FailedRows > 0)
                            {
                                _logger.Debug($"Update status to '{ImportStatus.FailedInvalidFile}' '{fileInfo.Name}' databaseId {databaseId}");
                                service.UpdateStatus(config, smallFileName, ImportStatus.FailedInvalidFile);
                            }
                            else
                            {
                                _logger.Debug($"Update status to '{ImportStatus.ProcessedSuppIntoleranceUpdate}' '{fileInfo.Name}' databaseId {databaseId}");
                                service.UpdateStatus(config, smallFileName, ImportStatus.ProcessedSuppIntoleranceUpdate);
                            }
                        }
                    }
                    catch (OleDbException oleDbEx)
                    {
                        _logger.Error("OleDbException", oleDbEx);
                        fileService.MoveFile(processingFileName, processedFileName);
                        service.UpdateStatus(config, smallFileName, ImportStatus.FailedInvalidTemplate);
                    }
                    catch (ArgumentException ex)
                    {
                        _logger.Error("ArgumentException", ex);
                        fileService.MoveFile(processingFileName, processedFileName);
                        if (ex.Message.Equals(string.Empty))
                            service.UpdateStatus(config, smallFileName, ImportStatus.FailedInvalidTemplate);
                        else
                            service.UpdateStatus(config, smallFileName, ImportStatus.FailedInvalidVersion);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Exception", ex);
                        fileService.MoveFile(processingFileName, processedFileName);
                        service.UpdateStatus(config, smallFileName, ImportStatus.FailedInvalidVersion);
                        throw;
                    }

                    SendProductMSMQMessage(service, databaseId, customerDbConnectionString);
                }
            }
        }

        private static void SendProductMSMQMessage(DataService.IngredientImportService service, int databaseId, string customerDbConnectionString)
        {
            List<Tuple<int?, string>> ingrIds = Start.CollectProductIds;

            var isRecipeEnabled = service.IsOrchestrationEnabled(Constants.EntityType.Ingredient);
            var isIngredientEnabled = service.IsOrchestrationEnabled(Constants.EntityType.Dish);
            //if one of two types are enabled get all affected products
            if (isRecipeEnabled || isIngredientEnabled)
            {
                var updatedEntities = service.GetUpdatedEntities(ingrIds)
                    .Where(c => (c.EntityTypeId == Constants.EntityType.Ingredient && isIngredientEnabled) || (c.EntityTypeId == Constants.EntityType.Dish && isRecipeEnabled))
                    .ToList();

                try
                {
                    int pageSize;
                    if (!int.TryParse(ConfigurationManager.AppSettings["pageSize"], out pageSize))
                    {
                        pageSize = 20;
                    }

                    if (!updatedEntities.Any()) return;
                    var groupedProducts = updatedEntities.GroupBy(c => new { c.EntityTypeId, c.MessageActionType });
                    foreach (var groupedProduct in groupedProducts)
                    {
                        var count = groupedProduct.Count();
                        var pages = (int)Math.Ceiling((double)count / pageSize);

                        for (var i = 0; i < pages; i++)
                        {
                            var skip = i == 0 ? i : i * pageSize;
                            var productIds = groupedProduct.OrderBy(c => c.EntityId).Skip(skip).Take(pageSize).Select(c => c.EntityId);
                            var jsonProductIds = JsonConvert.SerializeObject(productIds);
                            var entityType = groupedProduct.Key.EntityTypeId;
                            var messageActionType = groupedProduct.Key.MessageActionType;
                            MsmqHelper.Send(default(int), (int)entityType, databaseId, customerDbConnectionString, messageActionType, null, jsonProductIds);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    throw;
                }
            }
        }
    }
}

