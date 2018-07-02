using System.Data;
using Fourth.Import.Model;
using Fourth.Import.Mapping;
using Fourth.Import.Sql;
namespace Fourth.Import.Process
{
    public static class InjectSqlHelper
    {
        public static string QueryForProductId(MappingTable mappingTable, DataRow row)
        {
            if (mappingTable.TableName.ToUpperInvariant() != "INGREDIENT")
            {
                return null;
            }

            int? starchefKeyOrdinal = mappingTable.ColumnOrdinalOf("product_id");

            string starchefKey=string.Empty;
            
            if(starchefKeyOrdinal.HasValue)
            {
                starchefKey = row[(int) starchefKeyOrdinal].ToString();
            }
            if (string.IsNullOrWhiteSpace(starchefKey))
            {
                starchefKey = "0";
            }

            return string.Format(@"select @product_id = product_id,@ingredient_id = i.ingredient_id 
                                from ingredient i 
                                where i.product_id = {0}", starchefKey);
        }

        public static string SupImportQueryForProductId(MappingTable mappingTable, DataRow row)
        {
            if (mappingTable.TableName.ToUpperInvariant() != "INGREDIENT")
                return null;

            int? supplierCodeOrdinal = mappingTable.ColumnOrdinalOf("supplier_code");

            string supplierCode = string.Empty;

 
            
            if (supplierCodeOrdinal != null)
            {
                supplierCode = row[(int)supplierCodeOrdinal].ToString();
            }

 
            
            if (string.IsNullOrWhiteSpace(supplierCode))
            {
                supplierCode = "|=============|"; // this should never match with the database
            }

            //Following query is to make sure we update data only for correct suppliers and not disturbing other suppliers if they have same supplier code.
            return string.Format(@"select top 1 @product_id = product_id,@ingredient_id = i.ingredient_id from ingredient i 
                            where i.supplier_code = {0} and supplier_id IN (
                            select isupp.supplier_id from ingredient_import ii JOIN import_supplier isupp
                            ON ii.ingredient_import_id = isupp.ingredient_import_id
                            WHERE is_supplier_file = 1 and import_status_id =2 )", supplierCode.WithSqlSyntax(Datatype.String)
                );
        }

        public static string QueryForPbandId(MappingTable mappingTable, DataRow row)
        {
            if (mappingTable.TableName.ToUpperInvariant() != "PRODUCT_PBAND")
                return null;

            
            int? pbandKeyOrdinal = mappingTable.ColumnOrdinalOf("pband_id");

            string pbandName = string.Empty;
            if (pbandKeyOrdinal != null)
            {
                pbandName = row[(int)pbandKeyOrdinal].ToString();
            }

            return string.Format(@"select @pband_id = pband_id FROM pband WHERE pband_name like {0}", pbandName.WithSqlSyntax(Datatype.String));  
        }
       
    }
}