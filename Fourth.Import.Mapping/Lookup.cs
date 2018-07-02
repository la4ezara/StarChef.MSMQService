using System;
using System.Collections.Generic;
using Fourth.Import.Common;
using Fourth.Import.DataService;
using Fourth.Import.Model;

namespace Fourth.Import.Mapping
{
    public static class Lookup
    {
        public static IList<MappingColumn> LookupValues(this IList<MappingColumn> mappingColumns,Config config)
        {
            foreach (MappingColumn mappingColumn in mappingColumns)
            {
                switch (mappingColumn.LookupType)
                {
                    case LookupType.Table:
                    case LookupType.Tag:
                        using (var luService = new LookupService(config.TargetConnectionString))
                        {
                            mappingColumn.LookupValues = luService.Add(Convert.ToByte(mappingColumn.LookupTableId));
                        }
                        break;
                    case LookupType.Unit:
                        using (var ulService = new UnitLookupService(config.TargetConnectionString))
                        {
                            mappingColumn.LookupValues = ulService.Add(Convert.ToByte(mappingColumn.LookupTableId));
                        }
                        break;
                }
            }
            return mappingColumns;
        }
    }
}