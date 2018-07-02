using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using Fourth.Import.Common;
using Fourth.Import.DataService;
using Fourth.Import.ExcelService;
using Fourth.Import.Exceptions;
using Fourth.Import.Model;
using Fourth.Import.Mapping;
using Fourth.Import.Sql;

namespace Fourth.Import.Process
{
    public partial class Start
    {
        private Config _config;

        public static List<Tuple<int?, string>> CollectProductIds = new List<Tuple<int?, string>>();


        public ImportOperation Import(Config config)
        {
            _config = config;
            using (var operationTypeService = new OperationTypeService(config.TargetConnectionString))
            {
                var importType = operationTypeService.Get(config.ImportType);
                var currentOperation = importType.Operation;

                if (currentOperation == ImportOperation.Invalid)
                {
                    throw new ArgumentException("unknown Import type");
                }
                using (var iiService = new IngredientImportService(config.TargetConnectionString))
                {
                    if (config.ImportType == "Fourth_1.3" && iiService.ValidateStandardTemplate(config.FileName) == false)
                    {
                        throw new ArgumentException(string.Empty);
                    }
                    if (config.ImportType == "Fourth_1.6")
                    {
                        iiService.UpdateSaltTargetConfig();
                    }
                }

                // execute step before the import on the target DB
                if (!string.IsNullOrWhiteSpace(importType.PrerequisiteQuery))
                    operationTypeService.Exec(importType.PrerequisiteQuery);

                Insert(currentOperation, config);
                return currentOperation;
            }
        }

        public void Insert(ImportOperation importOperation, Config config)
        {
            IList<MappingTable> mappingTables = new Mapping.Setup().Load(config);

            DataTable dataTable = new ReaderService(config.ExcelImportConnectionString).Load(config.ExcelImportSheetName, config.ProductDataStartsFrom);

            List<ProcessedImportRow> exceptionsRows = new List<ProcessedImportRow>();
            DataTable spplierExceptionRows = new DataTable();

            if (config.ImportType == "Fourth_2.0")
            {
                 spplierExceptionRows = FindProductNotInReceivedFile(config, dataTable);
            }
            if (spplierExceptionRows.Rows.Count > 0)
            {
                DataTable exceptionReport = new ExceptionReport()
                    .Generate(spplierExceptionRows, config.DisplayFileName, config.ImportType);
                if (exceptionReport != null && exceptionReport.Rows.Count > 0)
                    new ExceptionReportService().Export(exceptionReport,
                        ConfigurationManager.AppSettings["ExceptionReportFilePath"] + config.ExceptionReportFileName);
                config.FailedRows = dataTable.Rows.Count;
                config.ProcessedRows = 0;
            }
            else
            {
                int? ingredientOrdinal = mappingTables.ColumnOrdinalOf("product", "product_name");
                int? ingredientOrdinalPk = mappingTables.ColumnOrdinalOf("ingredient", "product_id");
                int? supplierId = mappingTables.ColumnOrdinalOf("ingredient", "supplier_code");
                int rowNum = config.ProductDataStartsFrom;
                int processedRows = 0;
                int failedRows = 0;

                //Update starts from this column
                int startingCell = mappingTables.GetMinColumnOrdinal();
                CollectProductIds.Clear();

                foreach (DataRow row in dataTable.Rows)
                {
                    if (IsEmpty(row, startingCell))
                    {
                        rowNum++;
                        continue;
                    }
                    ProcessedImportRow processedImportRow = new ProcessedImportRow
                    {
                        Row = row,
                        IsValid = true,
                        ImportDataExceptions = new List<ImportDataException>(),
                        RowIndex = rowNum,
                        IngredientName =
                            ingredientOrdinal == null ? string.Empty : row[(int)ingredientOrdinal].ToString(),
                        IngredientId =
                            (ingredientOrdinalPk == null || ingredientOrdinalPk < 0) ? string.Empty : row[(int)ingredientOrdinalPk].ToString(),
                        DistributorCode =
                            (supplierId == null || supplierId <= 0) ? string.Empty : row[(int)supplierId].ToString(),
                    };
                   
                    GeneratorDelegates generatorDelegates = new GeneratorDelegates();

                    if (importOperation == ImportOperation.Insert)
                        generatorDelegates = ProductInsertDelegates(config);
                    else if (importOperation == ImportOperation.FuturePrice)
                        generatorDelegates = FuturePriceDelegates(config);
                    else if (importOperation == ImportOperation.InternalFuturePrice)
                        generatorDelegates = InternalFuturePriceDelegates(config);
                    else if (importOperation == ImportOperation.FuturePriceWithInvoiceCostPrice)
                        generatorDelegates = FuturePriceDelegates(config);
                    else if (importOperation == ImportOperation.IntolUpdate)
                        generatorDelegates = IntoleranceUpdateDelegates(config);
                    else if (importOperation == ImportOperation.NutritionUpdate)
                        generatorDelegates = NutrientUpdateDelegates(config);
                    else if (importOperation == ImportOperation.PriceOverride)
                        generatorDelegates = PriceOverrideDelegates(config);
                    else if (importOperation == ImportOperation.SupIntolUpdate)
                        generatorDelegates = SupIntoleranceUpdateDelegates(config);

                    bool result = new Sql.Generate().Query(mappingTables, processedImportRow, generatorDelegates);

                    if (processedImportRow.IsValid)
                    {
                        using (var ips = new ImportProductService(config.TargetConnectionString))
                        {
                            ips.ExecuteSql(processedImportRow.SqlQuery);

                            // add product to the queue for sending to MSMQ
                            if (!string.IsNullOrEmpty(processedImportRow.IngredientId) || !string.IsNullOrEmpty(processedImportRow.DistributorCode))
                            {
                                //New validation for stock count
                                int productId = 0;
                                if (int.TryParse(processedImportRow.IngredientId, out productId))
                                {
                                    ips.ValidateStockInventory(productId);
                                }

                                var item = !string.IsNullOrEmpty(processedImportRow.IngredientId)
                                    ? new Tuple<int?, string>(Convert.ToInt32(processedImportRow.IngredientId), null)
                                    : new Tuple<int?, string>(null, processedImportRow.DistributorCode);
                                CollectProductIds.Add(item);
                            }

                            processedRows++;
                        }
                    }
                    else
                    {
                        exceptionsRows.Add(processedImportRow);
                        failedRows++;
                    }
                    rowNum++;
                }

                List<MappingColumn> mappingColumns = MappingExtensions.GetTemplateColumns(mappingTables);
                using (var exService = new ExceptionMessageService(config.TargetConnectionString))
                {
                    List<ExceptionMessage> exceptionMessages = exService.GetExceptionMessages();

                    IList<int> dataRows;
                    DataTable exceptionReport = new ExceptionReport()
                        .Generate(mappingColumns, config.DisplayFileName, exceptionsRows, exceptionMessages,
                            config.ImportType, out dataRows);
                    if (exceptionReport != null && exceptionReport.Rows.Count > 0)
                        new ExceptionReportService().Export(exceptionReport,
                            ConfigurationManager.AppSettings["ExceptionReportFilePath"] + config.ExceptionReportFileName,
                            dataRows);
                    config.FailedRows = failedRows;
                    config.ProcessedRows = processedRows;
                }
            }
        }
        
        private GeneratorDelegates ProductInsertDelegates(Config config)
        {
            
            GeneratorDelegates del = new GeneratorDelegates
            {
                AuxillaryStartSql = () => string.Format("SET DATEFORMAT 'DMY' DECLARE @product_id INT, @ingredient_id int, @user_id INT, @ugroup_id INT,@audit_log_id INT SET @user_id = {0} SET @ugroup_id = {1}", config.ImportingUserId, config.ImportingUgroupId),
                                                            AuxillaryEndSql = () => string.Format("INSERT INTO product_pset(product_id, pset_id) select @product_id,pset_id from import_pset where ingredient_import_id = {0}", config.IngredientImportId),
                                                            TableValidations = new List<Func<MappingTable, DataRow, ImportDataException>>
                                                                                      {
                                                                                          ValidateUniqueDistributorCode,
                                                                                          ValidateUniqueSupplierCode
                                                                                      }
                                                        };
            if (config.ImportType != "Fourth_1.3" && config.ImportType != "Fourth_1.5")
            {
                del.ReplacementForTag = TagHelper.TagDelegate;
            }
            else
            {
                if (config.ImportType == "Fourth_1.5")
                {                    
                    del.TableValidations.Add(ValidateCostCentreCategory);
                    del.TableValidations.Add(ValidateDeliveryToWarehouseDate);
                }
                
                del.ReplacementForTag = TagHelper.TagDelegateTwoLevel;
                del.TableValidations.Add(ValidateProductClassification);
                del.TableValidations.Add(ValidateFlagExpiryDate);
                del.TableValidations.Add(ValidatePConversionUnit);
            }

            return del;
        }

        private GeneratorDelegates FuturePriceDelegates(Config config)
        {

            return new GeneratorDelegates
            {
                AuxillaryStartSql = () => string.Format("SET DATEFORMAT 'DMY' DECLARE @user_id INT, @product_id INT,@ingredient_id INT SET @user_id = {0} ", config.ImportingUserId),
                AuxillaryEndSql = () => string.Empty,
                InjectSql = new List<Func<MappingTable, DataRow, string>>
                                {
                                    InjectSqlHelper.QueryForProductId
                                },
                TableValidations = new List<Func<MappingTable, DataRow, ImportDataException>>
                                                                                      {
                                                                                          ProductExists,
                                                                                          DateIsValid,
                                                                                          FutureDateIsValid
                                                                                      }
            };

        }


        private GeneratorDelegates InternalFuturePriceDelegates(Config config)
        {

            return new GeneratorDelegates
            {
                AuxillaryStartSql = () => string.Format("SET DATEFORMAT 'DMY' DECLARE @user_id INT, @product_id INT, @ingredient_id INT SET @user_id = {0} ", config.ImportingUserId),
                AuxillaryEndSql = () => string.Empty,
                InjectSql = new List<Func<MappingTable, DataRow, string>>
                                {
                                    InjectSqlHelper.QueryForProductId
                                },
                TableValidations = new List<Func<MappingTable, DataRow, ImportDataException>>
                                                                                      {
                                                                                          ProductExists,
                                                                                          DateIsValid,
                                                                                          PriceValidUntilDateIsValid,
                                                                                          FutureDateIsValid,
                                                                                          PriceValidUntilFutureDateIsValid,
                                                                                          PubsPendingPriceEffectiveDateIsValid,
                                                                                          FranchisePendingPriceEffectiveDateIsValid
                                                                                      }
            };

        }

        private GeneratorDelegates IntoleranceUpdateDelegates(Config config)
        {

            return new GeneratorDelegates
            {
                AuxillaryStartSql = () => string.Format("SET DATEFORMAT 'DMY' DECLARE @user_id INT, @product_id INT,@ingredient_id INT SET @user_id = {0} ", config.ImportingUserId),
                AuxillaryEndSql = () => string.Empty,
                InjectSql = new List<Func<MappingTable, DataRow, string>>
                                {
                                    InjectSqlHelper.QueryForProductId
                                },
                TableValidations = new List<Func<MappingTable, DataRow, ImportDataException>>
                                                                                      {
                                                                                          ProductExists
                                                                                      }
            };

        }


        private GeneratorDelegates SupIntoleranceUpdateDelegates(Config config)
        {

            return new GeneratorDelegates
            {
                AuxillaryStartSql = () => string.Format("SET DATEFORMAT 'DMY' DECLARE @user_id INT, @product_id INT,@ingredient_id INT SET @user_id = {0} ", config.ImportingUserId),
                AuxillaryEndSql = () => string.Empty,
                InjectSql = new List<Func<MappingTable, DataRow, string>>
                                {
                                    InjectSqlHelper.SupImportQueryForProductId
                                },
                                TableValidations = new List<Func<MappingTable, DataRow, ImportDataException>>()
            };

        }

        private GeneratorDelegates NutrientUpdateDelegates(Config config)
        {

            return new GeneratorDelegates
            {
                AuxillaryStartSql = () => string.Format("SET DATEFORMAT 'DMY' DECLARE @user_id INT, @product_id INT,@ingredient_id INT SET @user_id = {0} ", config.ImportingUserId),
                AuxillaryEndSql = () => string.Empty,
                InjectSql = new List<Func<MappingTable, DataRow, string>>
                                {
                                    InjectSqlHelper.QueryForProductId
                                },
                TableValidations = new List<Func<MappingTable, DataRow, ImportDataException>>
                                                                                      {
                                                                                          ProductExists
                                                                                      }
            };

        }

        private GeneratorDelegates PriceOverrideDelegates(Config config)
        {

            return new GeneratorDelegates
            {
                AuxillaryStartSql = () => string.Format("SET DATEFORMAT 'DMY' DECLARE @user_id INT, @product_id INT,@ingredient_id INT, @pband_id SMALLINT SET @user_id = {0} ", config.ImportingUserId),
                AuxillaryEndSql = () => string.Empty,
                InjectSql = new List<Func<MappingTable, DataRow, string>>
                                {
                                    InjectSqlHelper.QueryForProductId,
                                    InjectSqlHelper.QueryForPbandId
                                },
                TableValidations = new List<Func<MappingTable, DataRow, ImportDataException>>
                                                                                      {
                                                                                          ProductExists
                                                                                      }
            };

        }

        private bool IsEmpty(DataRow row,int startingCell)
        {
            for (int cnt = startingCell; cnt < row.ItemArray.Length; cnt++)
            {
                if (!string.IsNullOrEmpty(row.ItemArray[cnt].ToString()))
                    return false;
            }
            return true;
        }

    }
}