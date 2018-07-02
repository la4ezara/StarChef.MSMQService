using System;
using System.Collections.Generic;
using System.Data;
using Fourth.Import.Data;
using Fourth.Import.Model;

namespace Fourth.Import.DataService
{
    public class ValidationRulesService : DalBase
    {
        public ValidationRulesService(string connectionString) : base(ProviderType.Sql,connectionString)
        {
        }

        
        public IList<ValidationRules> Add(int validationRuleId)
        {
            IDataParameter[] parameters = {
                                              GetParameter("@validation_id",validationRuleId)
                                          };

            const string sqlText = @"SELECT   UPPER(db_setting_field_attribute.db_setting_field_attribute) as db_setting_field_attribute,db_setting_field_lookup.db_setting_value
                            FROM   db_setting   
                            LEFT OUTER JOIN  db_setting_field_lookup   ON  db_setting.db_setting_id = db_setting_field_lookup.db_setting_id   
                            LEFT OUTER JOIN  db_setting_field_attribute  ON  db_setting_field_lookup.db_setting_field_attribute_id = db_setting_field_attribute.db_setting_field_attribute_id  
                            WHERE   db_setting.is_deleted = 0  and db_setting.db_setting_id = @validation_id";

            IList<ValidationRules> validationRules = new List<ValidationRules>();
            using (IDbConnection conn = GetConnection())
            {
                conn.Open();
                using (IDataReader dr = GetReader(conn, sqlText, parameters, CommandType.Text))
                {
                    ValidationRules rules = new ValidationRules();
                    while (dr.Read())
                    {
                        string attribute = dr.GetValue<string>("db_setting_field_attribute");
                        string value = dr.GetValue<string>("db_setting_value");

                        switch (attribute)
                        {
                            case DbSettingAttributeType.IsMandatory:
                                rules.Mandatory = value == "1" ? true : false;
                                break;
                            case DbSettingAttributeType.MinimumValue:
                                rules.MinimumValue = string.IsNullOrWhiteSpace(value) ? 0 : Convert.ToDecimal(value);
                                break;
                            case DbSettingAttributeType.MaximumValue:
                                rules.MaximumValue = string.IsNullOrWhiteSpace(value) ? 0 : Convert.ToDecimal(value);
                                break;
                            case DbSettingAttributeType.RegEx:
                                rules.RegEx = string.IsNullOrWhiteSpace(value) ? null : value;
                                break;
                            case DbSettingAttributeType.StringLength:
                                rules.StringLength = string.IsNullOrWhiteSpace(value) ? int.MaxValue : Convert.ToInt32(value);
                                break;
                        }
                    }
                    validationRules.Add(rules);
                }
            }
            return validationRules;
        }

        public bool Isrequired()
        {
            const string sqlText = @"SELECT [required] FROM root_tag_entity WHERE db_entity_id = 20 AND root_tag_id IN 
                                    (SELECT tag_id FROM tag WHERE tag_name = 'Ingredient Category' AND tag_parent_id IS NULL)";
            using (IDbConnection conn = GetConnection())
            {
                conn.Open();
                using (IDataReader dr = GetReader(conn, sqlText, CommandType.Text))
                {
                    if (dr.Read())
                        return dr.GetValue<bool>("required");
                }
            }
            return false;
        }
    }
}