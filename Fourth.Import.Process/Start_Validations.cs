using System;
using System.Data;
using System.Globalization;
using Fourth.Import.Common;
using Fourth.Import.DataService;
using Fourth.Import.Exceptions;
using Fourth.Import.Model;
using Fourth.Import.Mapping;
using System.Linq;

namespace Fourth.Import.Process
{
    public partial class Start
    {
        private ImportDataException ValidateUniqueDistributorCode(MappingTable mappingTable, DataRow row)
        {
            if (mappingTable.TableName.ToUpperInvariant() != "INGREDIENT")
                return null;

            int? distributorNameOrdinal = mappingTable.ColumnOrdinalOf("distributor_id");
            int? distributorCodeOrdinal = mappingTable.ColumnOrdinalOf("distributor_code");
            if (distributorNameOrdinal == null || distributorCodeOrdinal == null) return null;

            string importDistName = row[Convert.ToInt32(distributorNameOrdinal)].ToString().Trim();
            string importDistCode = row[Convert.ToInt32(distributorCodeOrdinal)].ToString().Trim();

            if (string.IsNullOrEmpty(importDistName) || string.IsNullOrEmpty(importDistCode)) return null;

            using (var sdService = new SuppDistService(_config.TargetConnectionString))
            {
                if (sdService.DistributorCodeUsedForProduct(importDistName, importDistCode))
                {
                    string columnName = mappingTable.ColumnNameOf("distributor_code");
                    string mappingName = mappingTable.MappingNameOf("distributor_code");
                    return new ImportDataException
                    {
                        ExceptionType = ExceptionType.DuplicateDistributorCode,
                        TemplateColumnName = columnName,
                        IsValid = false,
                        TemplateMappingColumn = mappingName
                    };
                }
            }
            return null;
        }

        public ImportDataException ValidateUniqueSupplierCode(MappingTable mappingTable, DataRow row)
        {
            if (mappingTable.TableName.ToUpperInvariant() != "INGREDIENT")
                return null;

            int? supplierNameOrdinal = mappingTable.ColumnOrdinalOf("supplier_id");
            int? supplierCodeOrdinal = mappingTable.ColumnOrdinalOf("supplier_code");
            if (supplierNameOrdinal == null || supplierCodeOrdinal == null) return null;

            string importSuppName = row[Convert.ToInt32(supplierNameOrdinal)].ToString().Trim();
            string importSuppCode = row[Convert.ToInt32(supplierCodeOrdinal)].ToString().Trim();

            if (string.IsNullOrEmpty(importSuppName) || string.IsNullOrEmpty(importSuppCode)) return null;
            using (var sdService = new SuppDistService(_config.TargetConnectionString))
            {
                if (sdService.SupplierCodeUsedForProduct(importSuppName, importSuppCode))
                {
                    string columnName = mappingTable.ColumnNameOf("supplier_code");
                    string mappingName = mappingTable.MappingNameOf("supplier_code");
                    return new ImportDataException
                    {
                        ExceptionType = ExceptionType.DuplicateDistributorCode,
                        TemplateColumnName = columnName,
                        IsValid = false,
                        TemplateMappingColumn = mappingName
                    };
                }
            }
            return null;
        }

        public ImportDataException ProductExists(MappingTable mappingTable, DataRow row)
        {
            if (mappingTable.TableName.ToUpperInvariant() != "INGREDIENT")
                return null;

            int? starchefKeyOrdinal = mappingTable.ColumnOrdinalOf("product_id");
            int? supplierNameOrdinal = mappingTable.ColumnOrdinalOf("supplier_id");
            int? supplierCodeOrdinal = mappingTable.ColumnOrdinalOf("supplier_code");

            string starchefKey = null;
            string supplierName = string.Empty;
            string supplierCode = string.Empty;

            if (starchefKeyOrdinal != null)
            {
                starchefKey = row[(int)starchefKeyOrdinal].ToString().Trim();
            }

            if (supplierNameOrdinal != null)
            {
                supplierName = row[(int)supplierNameOrdinal].ToString();
            }
            if (supplierCodeOrdinal != null)
            {
                supplierCode = row[(int)supplierCodeOrdinal].ToString();
            }

            if (string.IsNullOrWhiteSpace(supplierName))
            {
                supplierName = "";
            }
            if (string.IsNullOrWhiteSpace(supplierCode))
            {
                supplierCode = "|=============|"; // this should never match with the database	
            }

            using (var ipService = new ImportProductService(_config.TargetConnectionString))
            {
                if (!string.IsNullOrWhiteSpace(starchefKey) && !ipService.Valid(starchefKey))
                {
                    string columnName = mappingTable.ColumnNameOf("product_id");
                    string mappingName = mappingTable.MappingNameOf("product_id");
                    return new ImportDataException
                    {
                        ExceptionType = ExceptionType.NoRecordExistForUpdate,
                        TemplateColumnName = columnName,
                        IsValid = false,
                        TemplateMappingColumn = mappingName
                    };
                }
                else if (string.IsNullOrWhiteSpace(starchefKey) && !ipService.Valid(supplierName, supplierCode))
                {
                    string columnName = mappingTable.ColumnNameOf("supplier_id") + " or " + mappingTable.ColumnNameOf("supplier_code");
                    string mappingName = mappingTable.MappingNameOf("supplier_id") + mappingTable.MappingNameOf("supplier_code");
                    return new ImportDataException
                    {
                        ExceptionType = ExceptionType.NoRecordExistForUpdate,
                        TemplateColumnName = columnName,
                        IsValid = false,
                        TemplateMappingColumn = mappingName
                    };
                }

                return null;
            }
        }

        public ImportDataException DateIsValid(MappingTable mappingTable, DataRow row)
        {
            if (mappingTable.TableName.ToUpperInvariant() != "FUTURE_PRICE")
                return null;

            int? effectiveDateOrdinal = mappingTable.ColumnOrdinalOf("effective_date");
            string datestring="";
            if (effectiveDateOrdinal != null)
                datestring = row[(int) effectiveDateOrdinal].ToString().Trim();

            DateTime date;
            if(DateTime.TryParseExact(datestring, "d/M/yyyy", new CultureInfo("en-GB"), DateTimeStyles.None, out date))
                return null;
            if(DateTime.TryParseExact(datestring, "d MMMM yyyy", new CultureInfo("en-GB"), DateTimeStyles.None, out date))
                return null;
            if (DateTime.TryParseExact(datestring, "d MM yyyy", new CultureInfo("en-GB"), DateTimeStyles.None, out date))
                return null;
            if (DateTime.TryParseExact(datestring, "d MMM yyyy", new CultureInfo("en-GB"), DateTimeStyles.None, out date))
                return null;
            if (DateTime.TryParseExact(datestring, "d-MMM-yyyy", new CultureInfo("en-GB"), DateTimeStyles.None, out date))
                return null;
            if (DateTime.TryParseExact(datestring, "d-MMM-yy", new CultureInfo("en-GB"), DateTimeStyles.None, out date))
                return null;

            string columnName = mappingTable.ColumnNameOf("effective_date");
            string mappingName = mappingTable.MappingNameOf("effective_date");
            return new ImportDataException
                       {
                           ExceptionType = ExceptionType.InvalidDateFormat,
                           TemplateColumnName = columnName,
                           IsValid = false,
                           TemplateMappingColumn = mappingName
                       };
        }


        private ImportDataException PriceValidUntilDateIsValid(MappingTable mappingTable, DataRow row)
        {
            if (mappingTable.TableName.ToUpperInvariant() != "INGREDIENT_INTERNAL_SPECIFICATION")
                return null;

            int? effectiveDateOrdinal = mappingTable.ColumnOrdinalOf("price_valid_end_date");
            string datestring = "";
            if (effectiveDateOrdinal != null)
                datestring = row[(int)effectiveDateOrdinal].ToString().Trim();

            if (string.IsNullOrEmpty(datestring))
                return null;
                     
            DateTime date;
            if (DateTime.TryParseExact(datestring, "d/M/yyyy", new CultureInfo("en-GB"), DateTimeStyles.None, out date))
                return null;
            if (DateTime.TryParseExact(datestring, "d MMMM yyyy", new CultureInfo("en-GB"), DateTimeStyles.None, out date))
                return null;
            if (DateTime.TryParseExact(datestring, "d MM yyyy", new CultureInfo("en-GB"), DateTimeStyles.None, out date))
                return null;
            if (DateTime.TryParseExact(datestring, "d MMM yyyy", new CultureInfo("en-GB"), DateTimeStyles.None, out date))
                return null;
            if (DateTime.TryParseExact(datestring, "d-MMM-yyyy", new CultureInfo("en-GB"), DateTimeStyles.None, out date))
                return null;
            if (DateTime.TryParseExact(datestring, "d-MMM-yy", new CultureInfo("en-GB"), DateTimeStyles.None, out date))
                return null;

            string columnName = mappingTable.ColumnNameOf("price_valid_end_date");
            string mappingName = mappingTable.MappingNameOf("price_valid_end_date");

            return new ImportDataException
            {
                ExceptionType = ExceptionType.InvalidDateFormat,
                TemplateColumnName = columnName,
                IsValid = false,
                TemplateMappingColumn = mappingName
            };
        }


        public ImportDataException FutureDateIsValid(MappingTable mappingTable, DataRow row)
        {
            if (mappingTable.TableName.ToUpperInvariant() != "FUTURE_PRICE")
                return null;

            int? effectiveDateOrdinal = mappingTable.ColumnOrdinalOf("effective_date");
            string datestring = "";
            if (effectiveDateOrdinal != null)
                datestring = row[(int)effectiveDateOrdinal].ToString().Trim();

            DateTime date;
            string columnName = mappingTable.ColumnNameOf("effective_date");
            string mappingName = mappingTable.MappingNameOf("effective_date");
            int dateDays=0;

            if (DateTime.TryParseExact(datestring, "d/M/yyyy", new CultureInfo("en-GB"), DateTimeStyles.None, out date))
                dateDays = DateDiffFromToday(date);
            else if (DateTime.TryParseExact(datestring, "d MMMM yyyy", new CultureInfo("en-GB"), DateTimeStyles.None, out date))
                dateDays = DateDiffFromToday(date);
            else if (DateTime.TryParseExact(datestring, "d MM yyyy", new CultureInfo("en-GB"), DateTimeStyles.None, out date))
                dateDays = DateDiffFromToday(date);
            else if (DateTime.TryParseExact(datestring, "d MMM yyyy", new CultureInfo("en-GB"), DateTimeStyles.None, out date))
                dateDays = DateDiffFromToday(date);
            else if (DateTime.TryParseExact(datestring, "d-MMM-yyyy", new CultureInfo("en-GB"), DateTimeStyles.None, out date))
                dateDays = DateDiffFromToday(date);
            else if (DateTime.TryParseExact(datestring, "d-MMM-yy", new CultureInfo("en-GB"), DateTimeStyles.None, out date))
                dateDays = DateDiffFromToday(date);


            if (dateDays < 0)
            {
                return new ImportDataException
                           {
                               ExceptionType = ExceptionType.InvalidFutureDate,
                               TemplateColumnName = columnName,
                               IsValid = false,
                               TemplateMappingColumn = mappingName
                           };
            }

            return null;
        }

        public ImportDataException PubsPendingPriceEffectiveDateIsValid(MappingTable mappingTable, DataRow row)
        {
            if (mappingTable.TableName.ToUpperInvariant() != "INGREDIENT_INTERNAL_COST_MODEL_PENDING")
                return null;

            int? effectiveDateOrdinal = mappingTable.ColumnOrdinalOf("internal_effective_date_2");
            string datestring = "";
            if (effectiveDateOrdinal != null)
                datestring = row[(int)effectiveDateOrdinal].ToString().Trim();

            DateTime date;
            string columnName = mappingTable.ColumnNameOf("internal_effective_date_2");
            string mappingName = mappingTable.MappingNameOf("internal_effective_date_2");
            int dateDays = 0;
            bool isValid = false;

            if (DateTime.TryParseExact(datestring, "d/M/yyyy", new CultureInfo("en-GB"), DateTimeStyles.None, out date))
            {
                dateDays = DateDiffFromToday(date);
                isValid = true;
            }
            else if (DateTime.TryParseExact(datestring, "d MMMM yyyy", new CultureInfo("en-GB"), DateTimeStyles.None,
                out date))
            {
                dateDays = DateDiffFromToday(date);
                isValid = true;
            }
            else if (DateTime.TryParseExact(datestring, "d MM yyyy", new CultureInfo("en-GB"), DateTimeStyles.None,
                out date))
            {
                dateDays = DateDiffFromToday(date);
                isValid = true;
            }
            else if (DateTime.TryParseExact(datestring, "d MMM yyyy", new CultureInfo("en-GB"),
                DateTimeStyles.None, out date))
            {
                dateDays = DateDiffFromToday(date);
                isValid = true;
            }
            else if (DateTime.TryParseExact(datestring, "d-MMM-yyyy", new CultureInfo("en-GB"),
                DateTimeStyles.None, out date))
            {
                dateDays = DateDiffFromToday(date);
                isValid = true;
            }
            else if (DateTime.TryParseExact(datestring, "d-MMM-yy", new CultureInfo("en-GB"),
                DateTimeStyles.None, out date))
            {
                dateDays = DateDiffFromToday(date);
                isValid = true;
            }

            if (isValid == false && datestring.Trim().Length > 0)
            {
                return new ImportDataException
                {
                    ExceptionType = ExceptionType.InvalidDateFormat,
                    TemplateColumnName = columnName,
                    IsValid = false,
                    TemplateMappingColumn = mappingName
                };
            }
            if (dateDays < 0 )
            {
                return new ImportDataException
                {
                    ExceptionType = ExceptionType.InvalidFutureDate,
                    TemplateColumnName = columnName,
                    IsValid = false,
                    TemplateMappingColumn = mappingName
                };
            }

            return null;
        }

        public ImportDataException FranchisePendingPriceEffectiveDateIsValid(MappingTable mappingTable, DataRow row)
        {
            if (mappingTable.TableName.ToUpperInvariant() != "INGREDIENT_INTERNAL_COST_MODEL_PENDING")
                return null;

            int? effectiveDateOrdinal = mappingTable.ColumnOrdinalOf("internal_effective_date_3");
            string datestring = "";
            if (effectiveDateOrdinal != null)
                datestring = row[(int)effectiveDateOrdinal].ToString().Trim();

            DateTime date;
            string columnName = mappingTable.ColumnNameOf("internal_effective_date_3");
            string mappingName = mappingTable.MappingNameOf("internal_effective_date_3");
            int dateDays = 0;
            bool isValid = false;

            if (DateTime.TryParseExact(datestring, "d/M/yyyy", new CultureInfo("en-GB"), DateTimeStyles.None, out date))
            {
                dateDays = DateDiffFromToday(date);
                isValid = true;
            }
            else if (DateTime.TryParseExact(datestring, "d MMMM yyyy", new CultureInfo("en-GB"), DateTimeStyles.None,
                out date))
            {
                dateDays = DateDiffFromToday(date);
                isValid = true;
            }
            else if (DateTime.TryParseExact(datestring, "d MM yyyy", new CultureInfo("en-GB"), DateTimeStyles.None,
                out date))
            {
                dateDays = DateDiffFromToday(date);
                isValid = true;
            }
            else if (DateTime.TryParseExact(datestring, "d MMM yyyy", new CultureInfo("en-GB"),
                DateTimeStyles.None, out date))
            {
                dateDays = DateDiffFromToday(date);
                isValid = true;
            }
            else if (DateTime.TryParseExact(datestring, "d-MMM-yyyy", new CultureInfo("en-GB"),
                DateTimeStyles.None, out date))
            {
                dateDays = DateDiffFromToday(date);
                isValid = true;
            }
            else if (DateTime.TryParseExact(datestring, "d-MMM-yy", new CultureInfo("en-GB"),
                DateTimeStyles.None, out date))
            {
                dateDays = DateDiffFromToday(date);
                isValid = true;
            }

            if (isValid == false && datestring.Trim().Length > 0)
            {
                return new ImportDataException
                {
                    ExceptionType = ExceptionType.InvalidDateFormat,
                    TemplateColumnName = columnName,
                    IsValid = false,
                    TemplateMappingColumn = mappingName
                };
            }

            if (dateDays < 0)
            {
                return new ImportDataException
                {
                    ExceptionType = ExceptionType.InvalidFutureDate,
                    TemplateColumnName = columnName,
                    IsValid = false,
                    TemplateMappingColumn = mappingName
                };
            }

            return null;
        }

        public ImportDataException PriceValidUntilFutureDateIsValid(MappingTable mappingTable, DataRow row)
        {
            if (mappingTable.TableName.ToUpperInvariant() != "INGREDIENT_INTERNAL_SPECIFICATION")
                return null;

            int? effectiveDateOrdinal = mappingTable.ColumnOrdinalOf("price_valid_end_date");
            string datestring = "";
            if (effectiveDateOrdinal != null)
                datestring = row[(int)effectiveDateOrdinal].ToString().Trim();

            DateTime date;
            string columnName = mappingTable.ColumnNameOf("price_valid_end_date");
            string mappingName = mappingTable.MappingNameOf("price_valid_end_date");
            int dateDays = 0;

            if (DateTime.TryParseExact(datestring, "d/M/yyyy", new CultureInfo("en-GB"), DateTimeStyles.None, out date))
                dateDays = DateDiffFromToday(date);
            else if (DateTime.TryParseExact(datestring, "d MMMM yyyy", new CultureInfo("en-GB"), DateTimeStyles.None, out date))
                dateDays = DateDiffFromToday(date);
            else if (DateTime.TryParseExact(datestring, "d MM yyyy", new CultureInfo("en-GB"), DateTimeStyles.None, out date))
                dateDays = DateDiffFromToday(date);
            else if (DateTime.TryParseExact(datestring, "d MMM yyyy", new CultureInfo("en-GB"), DateTimeStyles.None, out date))
                dateDays = DateDiffFromToday(date);
            else if (DateTime.TryParseExact(datestring, "d-MMM-yyyy", new CultureInfo("en-GB"), DateTimeStyles.None, out date))
                dateDays = DateDiffFromToday(date);
            else if (DateTime.TryParseExact(datestring, "d-MMM-yy", new CultureInfo("en-GB"), DateTimeStyles.None, out date))
                dateDays = DateDiffFromToday(date);


            if (dateDays < 0)
            {
                return new ImportDataException
                {
                    ExceptionType = ExceptionType.InvalidFutureDate,
                    TemplateColumnName = columnName,
                    IsValid = false,
                    TemplateMappingColumn = mappingName
                };
            }

            return null;
        }

        private static int DateDiffFromToday(DateTime date)
        {
            TimeSpan timeSpan = date - DateTime.Now;
            if (timeSpan.Days != 0)
                return timeSpan.Days;
            else
                return timeSpan.Hours;
        }


        private ImportDataException ValidateProductClassification(MappingTable mappingTable, DataRow row)
        {
            if (mappingTable.TableName.ToUpperInvariant() != "INGREDIENT")
                return null;

            int? isNOnFoodOrdinal = mappingTable.ColumnOrdinalOf("is_non_food_item");
            int? productClassificationOrdinal = mappingTable.ColumnOrdinalOf("product_classification_id");
            if (isNOnFoodOrdinal == null || productClassificationOrdinal == null) return null;

            string isNonFood = row[Convert.ToInt32(isNOnFoodOrdinal)].ToString().Trim();
            string productClassification = row[Convert.ToInt32(productClassificationOrdinal)].ToString().Trim();

            if (string.IsNullOrEmpty(isNonFood)) return null;

            if (isNonFood.ToUpper() == "NON-FOOD" && string.IsNullOrEmpty(productClassification))
            {
                string columnName = mappingTable.ColumnNameOf("product_classification_id");
                string mappingName = mappingTable.MappingNameOf("product_classification_id");
                return new ImportDataException
                {
                    ExceptionType = ExceptionType.DbSettingIsMandatory,
                    TemplateColumnName = columnName,
                    IsValid = false,
                    TemplateMappingColumn = mappingName
                };
            }

            if(isNonFood.ToUpper() == "FOOD")
            {
                row[Convert.ToInt32(productClassificationOrdinal)] = string.Empty;
            }

            return null;
        }

        private ImportDataException ValidateFlagExpiryDate(MappingTable mappingTable, DataRow row)
        {
            if (mappingTable.TableName.ToUpperInvariant() != "PRODUCT")
                return null;

            int? flagTypeOrdinal = mappingTable.ColumnOrdinalOf("flag_type_id");
            int? flagExpiresOrdinal = mappingTable.ColumnOrdinalOf("flag_expires");
            if (flagTypeOrdinal == null || flagExpiresOrdinal == null) return null;

            string flagType = row[Convert.ToInt32(flagTypeOrdinal)].ToString().Trim();
            string flagExpires = row[Convert.ToInt32(flagExpiresOrdinal)].ToString().Trim();

            if (string.IsNullOrEmpty(flagType))
            {
                row[Convert.ToInt32(flagExpiresOrdinal)] = string.Empty;
                return null;
            }
            else
            {
                string columnName = mappingTable.ColumnNameOf("flag_expires");
                string mappingName = mappingTable.MappingNameOf("flag_expires");
                if (string.IsNullOrEmpty(flagExpires))
                {
                    return new ImportDataException
                    {
                        ExceptionType = ExceptionType.DbSettingIsMandatory,
                        TemplateColumnName = columnName,
                        IsValid = false,
                        TemplateMappingColumn = mappingName
                    };
                }
                else
                {
                    int dateDays = -2147483648;
                    DateTime date;

                    if (DateTime.TryParseExact(flagExpires, "d/M/yyyy", new CultureInfo("en-GB"), DateTimeStyles.None, out date))
                        dateDays = DateDiffFromToday(date);
                    else if (DateTime.TryParseExact(flagExpires, "d MMMM yyyy", new CultureInfo("en-GB"), DateTimeStyles.None, out date))
                        dateDays = DateDiffFromToday(date);
                    else if (DateTime.TryParseExact(flagExpires, "d MM yyyy", new CultureInfo("en-GB"), DateTimeStyles.None, out date))
                        dateDays = DateDiffFromToday(date);
                    else if (DateTime.TryParseExact(flagExpires, "d MMM yyyy", new CultureInfo("en-GB"), DateTimeStyles.None, out date))
                        dateDays = DateDiffFromToday(date);
                    else if (DateTime.TryParseExact(flagExpires, "d-MMM-yyyy", new CultureInfo("en-GB"), DateTimeStyles.None, out date))
                        dateDays = DateDiffFromToday(date);
                    else if (DateTime.TryParseExact(flagExpires, "d-MMM-yy", new CultureInfo("en-GB"), DateTimeStyles.None, out date))
                        dateDays = DateDiffFromToday(date);

                    if (dateDays == -2147483648 || date.Year > 2077)
                    {
                        return new ImportDataException
                        {
                            ExceptionType = ExceptionType.LookupMissing,
                            TemplateColumnName = columnName,
                            IsValid = false,
                            TemplateMappingColumn = mappingName
                        };
                    }

                    if (dateDays < 0)
                    {
                        return new ImportDataException
                        {
                            ExceptionType = ExceptionType.InvalidFutureDate,
                            TemplateColumnName = columnName,
                            IsValid = false,
                            TemplateMappingColumn = mappingName
                        };
                    }
                }
            }
            return null;
        }

        private static string suppQtyUnit;

        private ImportDataException ValidatePConversionUnit(MappingTable mappingTable, DataRow row)
        {
            if (mappingTable.TableName.ToUpperInvariant() != "PRODUCT" && mappingTable.TableName.ToUpperInvariant() != "PCONVERSION")
                return null;

            if (mappingTable.TableName.ToUpperInvariant() == "PRODUCT")
            {
                int? unitIdOrdinal = mappingTable.ColumnOrdinalOf("unit_id");
                if (unitIdOrdinal == null) return null;

                suppQtyUnit = row[Convert.ToInt32(unitIdOrdinal)].ToString().Trim();
            }
            else if (mappingTable.TableName.ToUpperInvariant() == "PCONVERSION")
            {
                if (string.IsNullOrEmpty(suppQtyUnit)) return null;
                int? unitIdOrdinal = mappingTable.ColumnOrdinalOf("unit_id");
                if (unitIdOrdinal == null) return null;
                string pconversionNum = row[Convert.ToInt32(unitIdOrdinal - 2)].ToString().Trim();
                string pconversionQty = row[Convert.ToInt32(unitIdOrdinal) - 1].ToString().Trim();
                string pconversionUnit = row[Convert.ToInt32(unitIdOrdinal)].ToString().Trim();

                if (string.IsNullOrEmpty(pconversionNum) || string.IsNullOrEmpty(pconversionNum) || string.IsNullOrEmpty(pconversionNum))
                {
                    mappingTable.MappingColumns[0].IgnoreThisTable = true;
                    return null;
                }

                using (var pcService = new PConversionService(_config.TargetConnectionString))
                {
                    if (!pcService.ValidatePConversionUnit(suppQtyUnit, pconversionUnit))
                    {
                        string columnName = mappingTable.ColumnNameOf("unit_id");
                        string mappingName = mappingTable.MappingNameOf("unit_id");
                        return new ImportDataException
                        {
                            ExceptionType = ExceptionType.InvalidPConversionUnit,
                            TemplateColumnName = columnName,
                            IsValid = false,
                            TemplateMappingColumn = mappingName
                        };
                    }
                }
            }
            return null;
        }

        private ImportDataException ValidateDeliveryToWarehouseDate(MappingTable mappingTable, DataRow row)
        {
            if (mappingTable.TableName.ToUpperInvariant() != "INGREDIENT_INTERNAL_SPECIFICATION")
                return null;

            int? deliveryToWarehouseDateOrdinal = mappingTable.ColumnOrdinalOf("delivery_to_warehouse_date");
            string datestring = "";
            if (deliveryToWarehouseDateOrdinal != null)
                datestring = row[(int)deliveryToWarehouseDateOrdinal].ToString().Trim();

            if (string.IsNullOrEmpty(datestring))
                return null;

            DateTime date;
            if (DateTime.TryParseExact(datestring, "d/M/yyyy", new CultureInfo("en-GB"), DateTimeStyles.None, out date))
                return null;
            if (DateTime.TryParseExact(datestring, "d MMMM yyyy", new CultureInfo("en-GB"), DateTimeStyles.None, out date))
                return null;
            if (DateTime.TryParseExact(datestring, "d MM yyyy", new CultureInfo("en-GB"), DateTimeStyles.None, out date))
                return null;
            if (DateTime.TryParseExact(datestring, "d MMM yyyy", new CultureInfo("en-GB"), DateTimeStyles.None, out date))
                return null;
            if (DateTime.TryParseExact(datestring, "d-MMM-yyyy", new CultureInfo("en-GB"), DateTimeStyles.None, out date))
                return null;
            if (DateTime.TryParseExact(datestring, "d-MMM-yy", new CultureInfo("en-GB"), DateTimeStyles.None, out date))
                return null;

            string columnName = mappingTable.ColumnNameOf("delivery_to_warehouse_date");
            string mappingName = mappingTable.MappingNameOf("delivery_to_warehouse_date");

            return new ImportDataException
            {
                ExceptionType = ExceptionType.InvalidDateFormat,
                TemplateColumnName = columnName,
                IsValid = false,
                TemplateMappingColumn = mappingName
            };
        }

        private ImportDataException ValidateCostCentreCategory(MappingTable mappingTable, DataRow row)
        {
            if (mappingTable.TableName.ToUpperInvariant() != "PRODUCT_TAG")
                return null;

            int? tagColumnIndex = mappingTable.ColumnOrdinalOf("tag_id");            

            if (tagColumnIndex == null) return null;

            string costCentreCategoryType = row[Convert.ToInt32(tagColumnIndex+2)].ToString().Trim();
            string costCentreMainCategory = row[Convert.ToInt32(tagColumnIndex+3)].ToString().Trim();
            string costCentreSubCategory = row[Convert.ToInt32(tagColumnIndex+4)].ToString().Trim();

            string columnName = mappingTable.ColumnNameOf("cost_centre_tag_type_id");
            string mappingName = mappingTable.MappingNameOf("cost_centre_tag_type_id");

            if (string.IsNullOrEmpty(costCentreCategoryType) || string.IsNullOrEmpty(costCentreMainCategory) || string.IsNullOrEmpty(costCentreSubCategory))
            {
                return new ImportDataException
                {
                    ExceptionType = ExceptionType.CategoryRequired,
                    TemplateColumnName = columnName,
                    IsValid = false,
                    TemplateMappingColumn = mappingName
                };
                //return null;
            }

            

            if (string.IsNullOrEmpty(costCentreCategoryType) && (!string.IsNullOrEmpty(costCentreMainCategory) || !string.IsNullOrEmpty(costCentreSubCategory)))
            {
                return new ImportDataException
                {
                    ExceptionType = ExceptionType.CategoryIsNotValid,
                    TemplateColumnName = columnName,
                    IsValid = false,
                    TemplateMappingColumn = mappingName
                };
            }

            using (var taglService = new TagLookupService(_config.TargetConnectionString))
            {
                int result = taglService.ValidateCostCentreCategory(costCentreCategoryType, costCentreMainCategory, costCentreSubCategory);
                if (result == 0)
                {

                    return new ImportDataException
                    {
                        ExceptionType = ExceptionType.CategoryIsNotValid,
                        TemplateColumnName = columnName,
                        IsValid = false,
                        TemplateMappingColumn = mappingName
                    };
                }
                else
                {
                    mappingTable.StartQueryWith = "DECLARE @cost_centre_tag_id INT; SET @cost_centre_tag_id =" + result.ToString().Trim();
                    //mappingTable.StartQueryWith.Replace("<@cost_centre_tag_id>", );
                }
            }
            return null;
        }

        private DataTable FindProductNotInReceivedFile(Config config, DataTable dataTable)
        {
            using (var sdService = new SuppDistService(config.TargetConnectionString)) {
                DataTable sc_products = sdService.GetAllSupplierProducts(config.FileName);

                var rowsOnlyInDt1 = sc_products.AsEnumerable().Where(r2 => !dataTable.AsEnumerable()
                        .Any(r => r[1].ToString().Trim().ToLower() == r2["supplier_code"].ToString().Trim().ToLower() && r[1].ToString().Trim().ToLower() == r2["supplier_code"].ToString().Trim().ToLower()));
                DataTable result = new DataTable();
                if (rowsOnlyInDt1.Count() > 0)
                    result = rowsOnlyInDt1.CopyToDataTable();//The third table

                return result;
            }
        }
    }
}