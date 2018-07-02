using System.Collections.Generic;
using Fourth.Import.Common;
using Fourth.Import.Model;

namespace Fourth.Import.Mapping
{
    public class Setup 
    {
        public IList<MappingTable> Load(Config config)
        {
            return new Table().Tables(config);
        }
    }
}