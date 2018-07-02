using System;
using System.Collections.Generic;
using System.Linq;
using Fourth.Import.Common;
using Fourth.Import.DataService;
using Fourth.Import.Model;

namespace Fourth.Import.Mapping
{
    public static class Validation
    {
        public static IList<MappingColumn> Rules(this IList<MappingColumn> mappingColumns, Config config)
        {
            using (var vrService = new ValidationRulesService(config.TargetConnectionString))
            {
                foreach (MappingColumn mappingColumn in mappingColumns.Where(mappingColumn => mappingColumn.ValidationSettingId != null || mappingColumn.StarchefColumnName == "tag_id"))
                {
                    mappingColumn.ValidationRules = vrService.Add(Convert.ToInt32(mappingColumn.ValidationSettingId));
                    if (mappingColumn.StarchefColumnName == "tag_id" && vrService.Isrequired())
                    {
                        mappingColumn.Mandatory = true;
                    }
                }
            }

            List<string> nutColumns = new List<string> {"N1", "N2", "N3", "N4", "N5", "N7", "N8", "N9", "N10", "N11", "N13", "N15","NF7"};
            
            foreach (MappingColumn mappingColumn in mappingColumns.Where(mappingColumn => mappingColumn.StarchefColumnName.StartsWith("N")))
            {
                if (nutColumns.Contains(mappingColumn.StarchefColumnName))
                {
                    ValidationRules nutValidation = new ValidationRules();
                    if (mappingColumn.StarchefColumnName == "NF7")
                    {
                        nutValidation.StringLength = 150;
                    }
                    else
                    {
                        nutValidation.MinimumValue = 0;
                        nutValidation.MaximumValue = 9999999;
                    }
                    mappingColumn.ValidationRules = new List<ValidationRules>();
                    mappingColumn.ValidationRules.Add(nutValidation);
                }
            }

            foreach (MappingColumn mappingColumn in mappingColumns.Where(mappingColumn => mappingColumn.StarchefColumnName == "price" && mappingColumn.TemplateMappingColumn == "PB2"))
            {
                ValidationRules numValidation = new ValidationRules();

                numValidation.MinimumValue = 0;
                numValidation.MaximumValue = 9999999;
                mappingColumn.ValidationRules = new List<ValidationRules>();
                mappingColumn.ValidationRules.Add(numValidation);
            }

            return mappingColumns;
        }
    }
}