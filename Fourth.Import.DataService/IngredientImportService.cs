using Fourth.Import.Common;
using Fourth.Import.Common.Messaging;
using Fourth.Import.Data;
using Fourth.Import.Model;
using Fourth.StarChef.Invariables;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Fourth.Import.DataService
{
    public class IngredientImportService : DalBase
    {
        public IngredientImportService(string connectionString)
            : base(ProviderType.Sql, connectionString)
        {
        }

        public Tuple<int, string> GetConnectionString(string smallFileName, string targetConnectionString)
        {
            IDataParameter[] parameters =
            {
                GetParameter("@file_name", smallFileName)
            };
            using (IDbConnection conn = GetConnection())
            {
                conn.Open();
                using (var dr = GetReader(conn, "sc_get_user_dsn", parameters, CommandType.StoredProcedure))
                {
                    if (!dr.Read())
                        throw new Exception("target database not found for this file");

                    var databaseId = DataReaderExtensions.GetValue<int>(dr, "db_database_id");
                    var catalog = DataReaderExtensions.GetValue<string>(dr, "database_name");
                    var server = DataReaderExtensions.GetValue<string>(dr, "server_name");

                    var connStr = catalog + ";" + server;

                    var items = server.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    var dataSource = items.FirstOrDefault(x => x.Contains("Data Source="));
                    if (!string.IsNullOrEmpty(dataSource))
                    {
                        connStr = string.Format(targetConnectionString, dataSource, catalog);
                    }

                    return new Tuple<int, string>(databaseId, connStr);
                }
            }
        }

        public Config GetImportInformation(string connStr, string smallFileName, string fileName)
        {
            IDataParameter[] parameters =
            {
                GetParameter("@sys_file_name", smallFileName)
            };
            using (IDbConnection conn = GetConnection())
            {
                conn.Open();
                using (IDataReader dr = GetReader(conn,"sc_get_ingredient_import", parameters, CommandType.StoredProcedure))
                {
                    if (dr.Read())
                    {
                        int importId = DataReaderExtensions.GetValue<int>(dr, "ingredient_import_id");
                        int userId = dr.GetInt32(1);
                        int uGroupId = dr.GetInt32(2);
                        string displayFileName = dr.GetString(3);

                        return new Config(connStr, importId, userId, uGroupId, fileName, displayFileName);
                    }
                }
            }
            throw new Exception("import details not found for this file");
        }

        public void UpdateStatus(Config config, string smallFileName, ImportStatus importStatus)
        {
            int processedRows = config == null ? 0 : config.ProcessedRows;
            int failedRows = config == null ? 0 : config.FailedRows;
            string exceptionReportFileName = config == null ? string.Empty : config.ExceptionReportFileName;
            IDataParameter[] parameters =
            {
                GetParameter("@file_name", smallFileName),
                GetParameter("@import_status", importStatus),
                GetParameter("@total_rows_saved",
                    config == null || config.ImportType == "Fourth_2.0" ? 0 : processedRows),
                GetParameter("@total_rows_failed", config == null || config.ImportType == "Fourth_2.0" ? 0 : failedRows),
                GetParameter("@error_file_name",
                (importStatus == ImportStatus.ProcessedNewIngredient ||
                 importStatus == ImportStatus.ProcessedPriceUpdate ||
                 importStatus == ImportStatus.ProcessedIntoleranceUpdate ||
                 importStatus == ImportStatus.ProcessedNutritionUpdate ||
                 importStatus == ImportStatus.ProcessedSuppIntoleranceUpdate ||
                 importStatus == ImportStatus.FailedInvalidFile) && failedRows > 0
                    ? exceptionReportFileName
                    : null)
            };

            ExecuteSql("sc_update_ingredient_import_status", parameters, CommandType.StoredProcedure);
        }

        public void UpdateIntolerances(Config config)
        {
            IDataParameter[] parameters =
            {
                GetParameter("@user_id", config.ImportingUserId)
            };

            ExecuteSql("sc_import_ingredients_intolerance_update", parameters, CommandType.StoredProcedure);
        }

        public bool IsOrchestrationEnabled(Constants.EntityType type) {
            bool result = false;
            IDataParameter[] parameters =
             {
                GetParameter("@entity_type_id", type)
            };
            using (IDbConnection conn = GetConnection())
            {
                conn.Open();
                using (var dr = GetReader(conn, "sc_get_orchestration_lookup", parameters, CommandType.StoredProcedure))
                {
                    while (dr.Read())
                    {
                        result = DataReaderExtensions.IsDBNull(dr, "is_publish") ? false : DataReaderExtensions.GetValue<bool>(dr, "is_publish");
                    }
                }
            }
            
            return result;
        }

        public IEnumerable<UpdatedEntityItem> GetUpdatedEntity(Tuple<int?, string> ids)
        {
            List<UpdatedEntityItem> resultList = new List<UpdatedEntityItem>();
            IDataParameter[] parameters =
             {
                GetParameter("@product_id", ids.Item1),
                GetParameter("@supplier_code", ids.Item2),
            };
            using (IDbConnection conn = GetConnection())
            {
                conn.Open();
                using (var dr = GetReader(conn, "sc_import_get_affected_products", parameters, CommandType.StoredProcedure))
                // todo create store procedure
                {
                    while (dr.Read())
                    {

                        var entityId = DataReaderExtensions.IsDBNull(dr, "product_id") ? 0 : DataReaderExtensions.GetValue<int>(dr, "product_id");
                        var entityTypeId = DataReaderExtensions.IsDBNull(dr, "product_type_id") ? 0 : DataReaderExtensions.GetValue<int>(dr, "product_type_id");

                        var item = new UpdatedEntityItem
                        {
                            EntityId = entityId,
                            EntityTypeId =
                                 entityTypeId == (int)Constants.ProductType.Ingredient
                                     ? Constants.EntityType.Ingredient
                                     : Constants.EntityType.Dish,
                            MessageActionType = Constants.MessageActionType.StarChefEventsUpdated
                        };

                        resultList.Add(item);
                    }

                    return resultList;
                }
            }
        }

        public IEnumerable<UpdatedEntityItem> GetUpdatedEntities(List<Tuple<int?, string>> ids)
        {
            List<UpdatedEntityItem> resultList = new List<UpdatedEntityItem>();


            foreach (var id in ids)
            {
                var itemsList = GetUpdatedEntity(id).ToList();
                resultList.AddRange(itemsList);
            }

            return resultList;
        }

        public void UpdateSupplierIntolerances(Config config)
        {
            IDataParameter[] parameters =
            {
                GetParameter("@user_id", config.ImportingUserId),
                GetParameter("@total_rows_failed", config.FailedRows)
            };

            ExecuteSql("sc_import_ingredients_supplier_intolerance_update", parameters, CommandType.StoredProcedure);
        }

        public void UpdateNutrients(Config config)
        {
            IDataParameter[] parameters =
            {
                GetParameter("@user_id", config.ImportingUserId)
            };

            ExecuteSql("sc_import_ingredients_nutrient_update", parameters, CommandType.StoredProcedure);
        }

        public bool ValidateStandardTemplate(string fileName)
        {
            IDataParameter[] parameters =
            {
                GetParameter("@file_name", fileName)
            };
            using (IDbConnection conn = GetConnection())
            {
                conn.Open();
                using (IDataReader dr = GetReader(conn, "sc_import_pset_validation", parameters, CommandType.StoredProcedure))
                {
                    if (dr.Read())
                    {
                        return dr.GetInt32(0) <= 0;
                    }
                }
            }
            return false;
        }


        public void UpdateSaltTargetConfig()
        {
            const string sqlText =
                @"UPDATE import_column_mapping SET is_mandatory = (SELECT TOP 1 CAST(ISNULL(db_setting_value,0) as BIT) FROM db_setting WHERE db_setting_name like 'CONFIG_SALT_TARGETS_REQUIRED') where column_name = 'NF9'";

            ExecuteSql(sqlText, CommandType.Text);
        }
        
    }
}
