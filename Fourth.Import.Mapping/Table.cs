using Fourth.Import.Common;
using Fourth.Import.DataService;
using Fourth.Import.Model;
using System.Collections.Generic;

namespace Fourth.Import.Mapping
{
    public class Table
    {
        public IList<MappingTable> Tables(Config config)
        {
            using (var mtService = new MappingTableService(config.TargetConnectionString))
            {
                IList<MappingTable> mappingTable = mtService.Load(config.ImportType);

                mappingTable.Columns(config).Add();
                return mappingTable;
            }
        }
    }
}